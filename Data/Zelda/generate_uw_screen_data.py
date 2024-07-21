import itertools

screen_data = [
    "", # 0x00 (00)
    "P", # 0x01 (01)
    "", # 0x02 (02)
    "", # 0x03 (03)
    "", # 0x04 (04)
    "", # 0x05 (05)
    "P", # 0x06 (06)
    "P", # 0x07 (07)
    "P", # 0x08 (08)
    "P", # 0x09 (09)
    "P", # 0x0A (10)
    "X", # 0x0B (11)
    "P", # 0x0C (12)
    "P", # 0x0D (13)
    "PX", # 0x0E (14)
    "R", # 0x0F (15)

    "P", # 0x10 (16)
    "P", # 0x11 (17)
    "X", # 0x12 (18)
    "X", # 0x13 (19)
    "X", # 0x14 (20)
    "",  # 0x15 (21)
    "X", # 0x16 (22)
    "",  # 0x17 (23)
    "X", # 0x18 (24)
    "X", # 0x19 (25)
    "PS", # 0x1A (26)
    "S", # 0x1B (27)
    "PS", # 0x1C (28)
    "P", # 0x1D (29)
    "P", # 0x1E (30)
    "P", # 0x1F (31)

    "PX", # 0x20 (32)
    "", # 0x21 (33)
    "P", # 0x22 (34)
    "", # 0x23 (35)
    "", # 0x24 (36)
    "", # 0x25 (37)
    "", # 0x26 (38)
    "PX", # 0x27 (39)
    "X", # 0x28 (40)
    "PX", # 0x29 (41)
]

for s in range(0, len(screen_data)):
    exits = []
    caves = []
    meta = []
    sd = screen_data[s]    

    print(f"Screen {s:02X}: {sd}")

    output =  f"name: Underworld Screen {s:02X}\n"
    output += f"area: Underworld\n"
    output += f"screen: 0x{s:02X}\n"
    output += "nodes:\n"
    output += "  exits:\n"
    #if "L" in sd:
    output += "    - name: \"Left edge\"\n"
    output += "      type: Scroll\n"
    output += "      direction: Left\n"
    output += "      \n"
    exits.append("Left edge")
#if "R" in sd:
    output += "    - name: \"Right edge\"\n"
    output += "      type: Scroll\n"
    output += "      direction: Right\n"
    output += "      \n"
    exits.append("Right edge")
#if "U" in sd:
    output += "    - name: \"Top ledge\"\n"
    output += "      type: Scroll\n"
    output += "      direction: Up\n"
    output += "      \n"
    exits.append("Top edge")
#if "D" in sd:
    output += "    - name: \"Bottom edge\"\n"
    output += "      type: Scroll\n"
    output += "      direction: Down\n"
    output += "      \n"
    exits.append("Bottom edge")

    output += "  meta:\n"
    output += "    - name: \"Middle\"\n"
    output += "      position: Middle\n"
    output += "      type: Meta\n"
    output += "      \n"

    if "P" in sd:
        output += "    - name: \"Push block\"\n"        
        output += "      type: Push\n"
        output += "      \n"

    if "S" in sd:
        output += "    - name: \"Stairs\"\n"
        output += "      type: Stairs\n"
        output += "      \n"

    if "X" in sd:
        output += "\n# TODO: This screen has special exit logic, needs to be implemented\n\n"

    # Generate fixed edges between all exits, caves and meta nodes
    # keeping in mind the requirement that each node has to be able to be
    # accessed. We start with the fixed nodes that's always present which is
    # the nodes from each Scroll exit to eachother
    output += "edges:\n"
    output += "  undirected:\n"
    output += "    fixed:\n"

    # get all combinations of exits
    for e1, e2 in itertools.combinations(exits, 2):
        output += f"      - [\"{e1}\", \"{e2}\"]\n"

    output += "      - [\"Left edge\", \"Middle\"]\n"

    if "P" in sd:
        output += "      - [\"Left edge\", \"Push block\"]\n"

    if "S" in sd:
        output += "      - [\"Left edge\", \"Stairs\"]\n"

    #f = open(f"Screens/Underworld/{s:02X}.yml", "w")
    #f.write(output)
    #f.close()
    



