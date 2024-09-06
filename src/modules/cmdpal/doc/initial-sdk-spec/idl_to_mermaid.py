import re
import sys
def parse_idl(file_path):
    """
    Parses a WinRT .idl file and extracts class, interface, method, property definitions,
    and inheritance relationships, associating them with their respective types.
    """
    with open(file_path, 'r') as file:
        content = file.read()

    # Regex patterns to identify class, interface, inheritance, methods, and properties
    class_pattern = re.compile(r'\b(runtimeclass|interface)\s+(\w+)(?:\s*:\s*([\w\s,]+))?\s*\{', re.MULTILINE)
    method_pattern = re.compile(r'\b(\w+)\s+(\w+)\s*\(([^)]*)\);', re.MULTILINE)
    property_pattern = re.compile(r'\b(\w+)\s+(\w+)\s*\{\s*(get;)?\s*(set;)?\s*\};', re.MULTILINE)

    # To store entities and their associated members
    entities = {}
    current_entity = None

    # Find all classes and interfaces with possible inheritance
    for class_match in class_pattern.finditer(content):
        entity_type, entity_name, inheritance = class_match.groups()
        current_entity = entity_name
        entities[current_entity] = {
            'type': entity_type,
            'methods': [],
            'properties': [],
            'inherits': [i.strip() for i in inheritance.split(',')] if inheritance else [],
            # 'content': content
        }

    # Find methods and properties within the class/interface bodies
    for entity_name in entities.keys():
        entity = entities[entity_name]
        # Use regex to find all methods and properties defined within each class/interface block
        entity_block_pattern = re.compile(rf'{entities[entity_name]["type"]}\s+{entity_name}\s*(?:\:\s*[\w\s,]+)?\s*\{{(.*?)\}}', re.MULTILINE | re.DOTALL)
        entity_block = entity_block_pattern.search(content)

        if entity_block:
            block_content = entity_block.group(1)

            # Find all methods in the current entity block
            for method_match in method_pattern.finditer(block_content):
                rtype, mname, params = method_match.groups()
                entities[entity_name]['methods'].append((rtype, mname, params))

            # Find all properties in the current entity block
            for property_match in property_pattern.finditer(block_content):
                ptype, pname, pget, pset = property_match.groups()
                entities[entity_name]['properties'].append((ptype, pname, pget, pset))

    return entities

def generate_mermaid_diagram(entities):
    """
    Generates a Mermaid classDiagram string from parsed IDL content.
    """
    mermaid_output = ["classDiagram"]

    # Process classes and interfaces
    for entity_name, entity_info in entities.items():
        mermaid_output.append(f"{entity_info['type']} {entity_name} {{")

        # Add properties
        for ptype, pname, pget, pset in entity_info['properties']:
            accessors = []
            if pget:
                accessors.append("get")
            if pset:
                accessors.append("set")
            accessors_str = "/" + ",".join(accessors) if accessors else ""
            mermaid_output.append(f"  {ptype} {pname}{accessors_str}")

        # Add methods
        for rtype, mname, params in entity_info['methods']:
            mermaid_output.append(f"  {rtype} {mname}({params})")

        mermaid_output.append("}")

    # Add inheritance relationships
    for entity_name, entity_info in entities.items():
        for base in entity_info['inherits']:
            mermaid_output.append(f"{base} <|-- {entity_name}")

    return "\n".join(mermaid_output)

def idl_to_mermaid(file_path):
    """
    Main function to convert IDL file to Mermaid classDiagram.
    """
    entities = parse_idl(file_path)
    mermaid_diagram = generate_mermaid_diagram(entities)
    return mermaid_diagram





############################################################################################



if __name__ == "__main__":
    # Replace 'example.idl' with your .idl file path
    idl_file_path = sys.argv[1]
    mermaid_diagram = idl_to_mermaid(idl_file_path)

    # Print the Mermaid diagram or write it to a file
    print(mermaid_diagram)

    # Optionally, save the diagram to a file
    with open("output.mmd", "w") as output_file:
        output_file.write(mermaid_diagram)
