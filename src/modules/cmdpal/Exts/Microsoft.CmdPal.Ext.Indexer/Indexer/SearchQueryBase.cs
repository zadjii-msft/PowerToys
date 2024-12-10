// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CmdPal.Ext.Indexer.Indexer.OleDB;
using Microsoft.CmdPal.Ext.Indexer.Indexer.Propsys;
using Microsoft.CmdPal.Ext.Indexer.Indexer.Utils;
using Microsoft.CmdPal.Ext.Indexer.Native;
using Microsoft.CmdPal.Ext.Indexer.Utils;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal abstract class SearchQueryBase
{
    private readonly Lock _lockObject = new(); // Lock object for synchronization

    private static readonly Guid IIDIRowsetInfo = new("0C733A55-2A1C-11CE-ADE5-00AA0044773D");

    private readonly DBPROPIDSET dbPropIdSet; // TODO: Implement IDisposable

    public uint ReuseWhereID { get; set; }

    public uint NumResults { get; set; }

    private IRowset currentRowset;
    private IRowset reuseRowset;

    public SearchQueryBase()
    {
        dbPropIdSet = new DBPROPIDSET
        {
            rgPropertyIDs = Marshal.AllocCoTaskMem(sizeof(uint)), // Allocate memory for the property ID array
            cPropertyIDs = 1,
            guidPropertySet = new Guid("AA6EE6B0-E828-11D0-B23E-00AA0047FC01"), // DBPROPSET_MSIDXS_ROWSETEXT,
        };

        // Copy the property ID into the allocated memory
        Marshal.WriteInt32(dbPropIdSet.rgPropertyIDs, 8); // MSIDXSPROP_WHEREID
    }

    protected virtual void FetchRows(out ulong totalFetched)
    {
        ulong fetched = 0;

        var getRow = currentRowset as IGetRow;
        uint rowCountReturned;

        ulong batchSize = 5000; // Number of rows to fetch in each batch

        do
        {
            IntPtr prghRows;
            var res = currentRowset.GetNextRows(
                IntPtr.Zero,       // hReserved
                0,                 // lRowsOffset
                (long)batchSize,         // cRows
                out rowCountReturned,  // pcRowsObtained
                out prghRows);

            if (res < 0)
            {
                Logger.LogError($"Error fetching rows: {res}");
                break;
            }

            if (rowCountReturned == 0 || rowCountReturned == IntPtr.Zero)
            {
                // No more rows to fetch
                break;
            }

            // Marshal the row handles
            var rowHandles = new IntPtr[rowCountReturned];
            Marshal.Copy(prghRows, rowHandles, 0, (int)rowCountReturned);

            ULongLongAdd(fetched, rowCountReturned, out fetched);

            for (ulong i = 0; i < rowCountReturned; i++)
            {
                object unknown;
                res = getRow.GetRowFromHROW(null, rowHandles[i], typeof(IPropertyStore).GUID, out unknown);
                if (res != 0)
                {
                    Logger.LogError($"Error getting row from HROW: {res}");
                    break;
                }

                var propStore = (IPropertyStore)unknown;

                NumResults++;
                OnFetchRowCallback(propStore);
            }

            // Release the rows
            res = currentRowset.ReleaseRows(
                rowCountReturned,
                rowHandles,
                IntPtr.Zero,
                null,
                null);

            if (res != 0)
            {
                Logger.LogError($"Error releasing rows: {res}");
                break;
            }

            // Free the memory allocated for row handles
            Marshal.FreeCoTaskMem(prghRows);
        }
        while (rowCountReturned > 0);

        totalFetched = fetched;
    }

    protected void ExecuteQueryStringSync(string queryStr)
    {
        try
        {
            lock (_lockObject)
            {
                // We need to generate a search query string with the search text the user entered above
                if (currentRowset != null)
                {
                    if (reuseRowset != null)
                    {
                        Marshal.ReleaseComObject(reuseRowset);
                    }

                    // We have a previous rowset, this means the user is typing and we should store this
                    // recapture the where ID from this so the next ExecuteSync call will be faster
                    reuseRowset = currentRowset;
                    ReuseWhereID = GetReuseWhereId(reuseRowset);
                }

                currentRowset = ExecuteCommand(queryStr);

                OnPreFetchRows();
                FetchRows(out var rowsFetched);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Error executing query", ex);
        }
        finally
        {
            OnPostFetchRows();
        }
    }

    protected void PrimeIndexAndCacheWhereId()
    {
        var queryStr = QueryStringBuilder.GeneratePrimingQuery();
        var rowset = ExecuteCommand(queryStr);
        if (rowset != null)
        {
            if (reuseRowset != null)
            {
                Marshal.ReleaseComObject(reuseRowset);
            }

            reuseRowset = rowset;
        }

        ReuseWhereID = GetReuseWhereId(reuseRowset);
    }

    private IRowset ExecuteCommand(string queryStr)
    {
        object sessionPtr = null;
        object commandPtr = null;

        try
        {
            var session = (IDBCreateSession)DataSourceManager.GetDataSource();
            var hr = session.CreateSession(null, typeof(IDBCreateCommand).GUID, out sessionPtr);
            if (hr != 0 || sessionPtr == null)
            {
                Logger.LogError("CreateSession failed: " + hr);
                return null;
            }

            var createCommand = (IDBCreateCommand)sessionPtr;
            hr = createCommand.CreateCommand(null, typeof(ICommandText).GUID, out commandPtr);
            if (hr != 0 || commandPtr == null)
            {
                Logger.LogError("CreateCommand failed: " + hr);
                return null;
            }

            var commandText = (ICommandText)commandPtr;
            if (commandText == null)
            {
                Logger.LogError("Failed to get ICommandText interface");
                return null;
            }

            Guid dbGuidDefault = new("C8B521FB-5CF3-11CE-ADE5-00AA0044773D");
            var res = commandText.SetCommandText(ref dbGuidDefault, queryStr);
            if (res != 0)
            {
                var err = NativeHelpers.GetErrorInfo(0, out var errorInfo);
                if (err == 0 && errorInfo != null)
                {
                    errorInfo.GetDescription(out var description);
                    Logger.LogError($"SetCommandText Error: {description}");
                }

                return null;
            }

            res = commandText.Execute(null, typeof(IRowset).GUID, 0, out var _, out var rowsetPointer);
            if (res != 0)
            {
                Logger.LogError($"Execute Error: {res}");
                return null;
            }

            return rowsetPointer as IRowset;
        }
        catch (Exception ex)
        {
            Logger.LogError("Unexpected error.", ex);
            return null;
        }
        finally
        {
            // Release the command pointer
            if (commandPtr != null)
            {
                Marshal.ReleaseComObject(commandPtr);
            }

            // Release the session pointer
            if (sessionPtr != null)
            {
                Marshal.ReleaseComObject(sessionPtr);
            }
        }
    }

    private IRowsetInfo GetRowsetInfo(IRowset rowset)
    {
        if (rowset == null)
        {
            return null;
        }

        var rowsetInfoPtr = IntPtr.Zero;

        try
        {
            // Get the IUnknown pointer for the IRowset object
            var rowsetPtr = Marshal.GetIUnknownForObject(rowset);

            // Query for IRowsetInfo interface
            var res = Marshal.QueryInterface(rowsetPtr, in IIDIRowsetInfo, out rowsetInfoPtr);
            if (res != 0)
            {
                Logger.LogError($"Error getting IRowsetInfo interface: {res}");
                return null;
            }

            // Marshal the interface pointer to the actual IRowsetInfo object
            var rowsetInfo = (IRowsetInfo)Marshal.GetObjectForIUnknown(rowsetInfoPtr);
            return rowsetInfo;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Exception occurred while getting IRowsetInfo. ", ex);
            return null;
        }
        finally
        {
            // Release the IRowsetInfo pointer if it was obtained
            if (rowsetInfoPtr != IntPtr.Zero)
            {
                Marshal.Release(rowsetInfoPtr); // Release the IRowsetInfo pointer
            }
        }
    }

    private DBPROP? GetPropset(IRowsetInfo rowsetInfo)
    {
        var prgPropSetsPtr = IntPtr.Zero;

        try
        {
            ulong cPropertySets;
            var res = rowsetInfo.GetProperties(1, [dbPropIdSet], out cPropertySets, out prgPropSetsPtr);
            if (res != 0)
            {
                Logger.LogError($"Error getting properties: {res}");
                return null;
            }

            if (cPropertySets == 0 || prgPropSetsPtr == IntPtr.Zero)
            {
                Logger.LogError("No property sets returned");
                return null;
            }

            var firstPropSetPtr = new IntPtr(prgPropSetsPtr.ToInt64());
            var propSet = Marshal.PtrToStructure<DBPROPSET>(firstPropSetPtr);
            if (propSet.cProperties == 0 || propSet.rgProperties == IntPtr.Zero)
            {
                return null;
            }

            var propPtr = new IntPtr(propSet.rgProperties.ToInt64());
            var prop = Marshal.PtrToStructure<DBPROP>(propPtr);
            return prop;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Exception occurred while getting properties,", ex);
            return null;
        }
        finally
        {
            // Free the property sets pointer returned by GetProperties, if necessary
            if (prgPropSetsPtr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(prgPropSetsPtr);
            }
        }
    }

    private uint GetReuseWhereId(IRowset rowset)
    {
        var rowsetInfo = GetRowsetInfo(rowset);
        if (rowsetInfo == null)
        {
            return 0;
        }

        var prop = GetPropset(rowsetInfo);
        if (prop == null)
        {
            return 0;
        }

        if (prop?.vValue.vt == (ushort)VarEnum.VT_UI4)
        {
            var value = prop?.vValue.unionValue.ulVal;
            return (uint)value;
        }

        return 0;
    }

    public abstract void OnPreFetchRows();

    public abstract void OnPostFetchRows();

    public abstract void OnFetchRowCallback(IPropertyStore propStore);

    private static bool ULongLongAdd(ulong augend, ulong addend, out ulong result)
    {
        var bigSum = (BigInteger)augend + addend;

        if (bigSum <= ulong.MaxValue)
        {
            result = (ulong)bigSum;
            return true;
        }
        else
        {
            result = 0;
            return false; // Overflow occurred
        }
    }

    /*public void Dispose()
    {
        // Free the allocated memory for rgPropertyIDs
        if (propset.rgPropertyIDs != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(propset.rgPropertyIDs);
        }
    }*/
}
