// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Microsoft.CmdPal.Ext.Indexer.Indexer.OleDB;
using Microsoft.CmdPal.Ext.Indexer.Indexer.Propsys;
using Microsoft.CmdPal.Ext.Indexer.Utils;
using MSDASC;

namespace Microsoft.CmdPal.Ext.Indexer.Indexer;

internal abstract class SearchQueryBase
{
    private readonly object _lockObject = new(); // Lock object for synchronization

    public uint ReuseWhereID { get; set; }

    public uint NumResults { get; set; }

    private IRowset currentRowset;

    private IRowset reuseRowset;

    public SearchQueryBase()
    {
    }

    protected virtual void FetchRows(out ulong totalFetched)
    {
        ulong fetched = 0;

        IGetRow getRow = currentRowset as IGetRow;
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
            // Marshal.FreeCoTaskMem(prghRows);
        }
        while (rowCountReturned > 0);

        totalFetched = fetched;
    }

    [DllImport("oleaut32.dll")]
    private static extern int GetErrorInfo(uint dwReserved, out IErrorInfo ppErrorInfo);

    protected void ExecuteQueryStringSync(string queryStr)
    {
        try
        {
            lock (_lockObject)
            {
                // We need to generate a search query string with the search text the user entered above
                if (currentRowset != null)
                {
                    // We have a previous rowset, this means the user is typing and we should store this
                    // recapture the where ID from this so the next ExecuteSync call will be faster
                    reuseRowset = currentRowset;
                    ReuseWhereID = GetReuseWhereId(reuseRowset);
                }

                currentRowset = null;

                ICommandText cmdTxt;
                GetCommandText(out cmdTxt);
                var dbGuidDefault = new Guid("C8B521FB-5CF3-11CE-ADE5-00AA0044773D");
                var res = cmdTxt.SetCommandText(ref dbGuidDefault, queryStr);
                if (res != 0)
                {
                    // TODO: log error
                    var err = GetErrorInfo(0, out IErrorInfo errorInfo);
                    if (err == 0 && errorInfo != null)
                    {
                        errorInfo.GetDescription(out var description);
                        Logger.LogError($"SetCommandText Error: {description}");
                    }

                    return;
                }

                res = cmdTxt.Execute(null, typeof(IRowset).GUID, 0, out var rowCount, out var unkRowsetPtr);
                if (res != 0)
                {
                    Logger.LogError($"Execute Error: {res}");
                    return;
                }

                currentRowset = unkRowsetPtr as IRowset;

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
        // We need to generate a search query string with the search text the user entered above
        var queryStr = GetPrimingQueryString();

        ICommandText cmdTxt;
        GetCommandText(out cmdTxt);
        if (cmdTxt == null)
        {
            Logger.LogError("Failed to get ICommandText interface");
            return;
        }

        var dbGuidDefault = new Guid("C8B521FB-5CF3-11CE-ADE5-00AA0044773D");
        var res = cmdTxt.SetCommandText(ref dbGuidDefault, queryStr);
        if (res != 0)
        {
            Logger.LogError($"SetCommandText Error: {res}");
            return;
        }

        res = cmdTxt.Execute(null, typeof(IRowset).GUID, 0, out var _, out var unkRowsetPtr);
        if (res != 0)
        {
            Logger.LogError($"Execute Error: {res}");
            return;
        }

        reuseRowset = unkRowsetPtr as IRowset;
        ReuseWhereID = GetReuseWhereId(reuseRowset);
    }

    protected void GetCommandText(out ICommandText cmdText)
    {
        // Query CommandText
        const string CLSID_CollatorDataSource = "9E175B8B-F52A-11D8-B9A5-505054503030";

        var hr = CoCreateInstance(
            new Guid(CLSID_CollatorDataSource), // CLSID_CollatorDataSource
            null,
            0x1, // CLSCTX_INPROC_SERVER
            typeof(IDBInitialize).GUID,
            out var dataSourceObj);

        if (hr != 0)
        {
            Logger.LogError("CoCreateInstance failed: " + hr);
            cmdText = null;
            return;
        }

        IDBInitialize dataSource = (IDBInitialize)dataSourceObj;
        hr = dataSource.Initialize();
        if (hr != 0)
        {
            Logger.LogError("DB Initialize failed: " + hr);
            cmdText = null;
            return;
        }

        IDBCreateSession session = (IDBCreateSession)dataSource;
        object unkSessionPtr;

        hr = session.CreateSession(null, typeof(IDBCreateCommand).GUID, out unkSessionPtr);
        if (hr != 0)
        {
            Logger.LogError("CreateSession failed: " + hr);
            cmdText = null;
            return;
        }

        IDBCreateCommand createCommand = (IDBCreateCommand)unkSessionPtr;

        hr = createCommand.CreateCommand(null, typeof(ICommandText).GUID, out var unkCmdPtr);
        if (hr != 0)
        {
            Logger.LogError("CreateCommand failed: " + hr);
            cmdText = null;
            return;
        }

        cmdText = (ICommandText)unkCmdPtr;
    }

    protected uint GetReuseWhereId(IRowset rowset)
    {
        // Get the IUnknown pointer for the IRowset object
        var rowsetPtr = Marshal.GetIUnknownForObject(rowset);
        IntPtr rowsetInfoPtr;
        var iidIRowsetInfo = new Guid("0C733A55-2A1C-11CE-ADE5-00AA0044773D"); // IID_IRowsetInfo
        var res = Marshal.QueryInterface(rowsetPtr, in iidIRowsetInfo, out rowsetInfoPtr);
        Marshal.Release(rowsetPtr);

        // var res = rowset.QueryInterface(typeof(IRowsetInfo).GUID, out var rowsetInfoObj);
        if (res != 0)
        {
            Logger.LogError($"Error getting IRowsetInfo interface: {res}");
            return 0;
        }

        // Get the IRowsetInfo object from the interface pointer
        // IRowsetInfo rowsetInfo = (IRowsetInfo)rowsetInfoObj;
        IRowsetInfo rowsetInfo = (IRowsetInfo)Marshal.GetObjectForIUnknown(rowsetInfoPtr);

        DBPROPIDSET propset = new DBPROPIDSET
        {
            // rgPropertyIDs = new uint[] { 8 }, // MSIDXSPROP_WHEREID,
            rgPropertyIDs = Marshal.AllocCoTaskMem(sizeof(uint)), // Allocate memory for the property ID array
            cPropertyIDs = 1,
            guidPropertySet = new Guid("AA6EE6B0-E828-11D0-B23E-00AA0047FC01"), // DBPROPSET_MSIDXS_ROWSETEXT,
        };

        // Copy the property ID into the allocated memory
        Marshal.WriteInt32(propset.rgPropertyIDs, 8); // MSIDXSPROP_WHEREID

        var prgPropSetsPtr = IntPtr.Zero;

        try
        {
            ulong cPropertySets;

            res = rowsetInfo.GetProperties(1, new DBPROPIDSET[] { propset }, out cPropertySets, out prgPropSetsPtr);
            if (res != 0)
            {
                Logger.LogError($"Error getting properties: {res}");
                return 0;
            }

            if (cPropertySets == 0 || prgPropSetsPtr == IntPtr.Zero)
            {
                Logger.LogError("No property sets returned");
                return 0;
            }

            // Marshal the returned DBPROPSET
            var dbPropSetSize = Marshal.SizeOf(typeof(DBPROPSET));
            DBPROPSET[] propSets = new DBPROPSET[cPropertySets];

            for (var i = 0; i < (int)cPropertySets; i++)
            {
                var currentPtr = new IntPtr(prgPropSetsPtr.ToInt64() + (i * dbPropSetSize));
                propSets[i] = Marshal.PtrToStructure<DBPROPSET>(currentPtr);
            }

            DBPROPSET propSet = propSets[0];
            if (propSet.cProperties == 0 || propSet.rgProperties == IntPtr.Zero)
            {
                // FreeProperties(cPropertySets, prgPropSetsPtr, propSets);
                return 0;
            }

            var dbPropSize = Marshal.SizeOf(typeof(DBPROP));
            DBPROP[] props = new DBPROP[propSet.cProperties];

            for (var j = 0; j < (int)propSet.cProperties; j++)
            {
                var propPtr = new IntPtr(propSet.rgProperties.ToInt64() + (j * dbPropSize));
                props[j] = Marshal.PtrToStructure<DBPROP>(propPtr);
            }

            // Access the property value
            DBPROP prop = props[0];
            if (prop.vValue.vt == (ushort)VarEnum.VT_UI4)
            {
                var value = prop.vValue.unionValue.ulVal;

                // Free allocated memory before returning
                // FreeProperties(cPropertySets, prgPropSetsPtr, propSets);
                return value;
            }
            else
            {
                // FreeProperties(cPropertySets, prgPropSetsPtr, propSets);
                return 0;
            }
        }
        finally
        {
            // Free the allocated memory for rgPropertyIDs
            if (propset.rgPropertyIDs != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(propset.rgPropertyIDs);
            }

            // Free the property sets pointer returned by GetProperties, if necessary
            if (prgPropSetsPtr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(prgPropSetsPtr);
            }
        }
    }

    public abstract void OnPreFetchRows();

    public abstract void OnPostFetchRows();

    public abstract void OnFetchRowCallback(IPropertyStore propStore);

    public abstract string GetPrimingQueryString();

    private static bool ULongLongAdd(ulong augend, ulong addend, out ulong result)
    {
        BigInteger bigSum = (BigInteger)augend + addend;

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

    [DllImport("ole32.dll", CharSet = CharSet.Unicode)]
    private static extern int CoCreateInstance(
        [In] Guid rclsid,
        [MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter,
        uint dwClsContext,
        [In] Guid riid,
        [MarshalAs(UnmanagedType.IUnknown)] out object ppv);
}
