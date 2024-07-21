import itertools

screen_data = [    
    "DRB", # 0x00 (0)
    "LRDB", # 0x01 (1)
    "LRDB", # 0x02 (2)
    "LDB", # 0x03 (3)
    "RC", # 0x04 (4)
    "LRB", # 0x05 (5)
    "LR", # 0x06 (6)
    "LRDB", # 0x07 (7)
    "LP", # 0x08 (8)
    "DC", # 0x09 (9)
    "DC", # 0x0A (10)
    "RDC", # 0x0B (11)
    "LDB", # 0x0C (12)
    "DC", # 0x0D (13)
    "DC", # 0x0E (14)
    "RU",  # 0x0F (15)

    "LUP", # 0x10 (16)  
    "RU", # 0x11 (17)
    "LRU", # 0x12 (18)
    "LRDBA", # 0x13 (19)
    "LRDBX", # 0x14 (20)
    "LRDB", # 0x15 (21)
    "LUSDR", # 0x16 (22)
    "LR", # 0x17 (23)
    "LR", # 0x18 (24)
    "LRU", # 0x19 (25)
    "LRUDP", # 0x1A (26)
    "LRUDA", # 0x1B (27)
    "LRUDPX", # 0x1C (28)
    "LRUB", # 0x1D (29)
    "LC", # 0x1E (30)
    "RDG",  # 0x1F (31)

    "LDG", # 0x20 (32)
    "DC", # 0x21 (33)
    "RDPX", # 0x22 (34)
    "LRUA", # 0x23 (35)
    "LUDACX", # 0x24 (36)
    "RDB", # 0x25 (37)
    "LSRUB", # 0x26 (38)
    "LRDT", # 0x27 (39)
    "LR", # 0x28 (40)
    "LRD", # 0x29 (41)
    "LRD", # 0x2A (42)
    "LRUB", # 0x2B (43)
    "LRUDBX", # 0x2C (44)
    "LD", # 0x2D (45)
    "DC", # 0x2E (46)
    "RUD", # 0x2F (47)

    "LRUD", # 0x30 (48)
    "LRU", # 0x31 (49)
    "LUAX", # 0x32 (50)
    "DA", # 0x33 (51)
    "UR", # 0x34 (52)
    "UL", # 0x35 (53)
    "RC", # 0x36 (54)
    "LUD", # 0x37 (55)
    "DF", # 0x38 (56)
    "RUD", # 0x39 (57)
    "LUD", # 0x3A (58)
    "UDA", # 0x3B (59)
    "UR", # 0x3C (60)
    "LDUX", # 0x3D (61)
    "RUD", # 0x3E (62) 
    "LU",   # 0x3F (63)

    "UDC", # 0x40 (64)
    "DC", # 0x41 (65)
    "RDT", # 0x42 (66)
    "LRT", # 0x43 (67)
    "LUDT", # 0x44 (68)
    "RUDP", # 0x45 (69)
    "LRUC", # 0x46 (70)
    "LUDTX", # 0x47 (71)
    "RU", # 0x48 (72)
    "LRUDT", # 0x49 (73)
    "LA", # 0x4A (74)
    "UDI", # 0x4B (75)
    "UD", # 0x4C (76)
    "RDT", # 0x4D (77)
    "LRUD", # 0x4E (78)
    "LRUDT", # 0x4F (79)
    
    "LRUD", # 0x50 (80)
    "LRUDX", # 0x51 (81)
    "LRUT", # 0x52 (82)
    "LR", # 0x53 (83)
    "LRUD", # 0x54 (84)
    "LRUD", # 0x55 (85)
    "LRD", # 0x56 (86)
    "LRUDT", # 0x57 (87)
    "LRDX", # 0x58 (88)
    "LRUD", # 0x59 (89)
    "LC", # 0x5A (90)
    "RU", # 0x5B (91)
    "LRUD", # 0x5C (92)
    "LRUDTX", # 0x5D (93)
    "LRUDT", # 0x5E (94)
    "LRUC", # 0x5F (95)
    
    "LRUD", # 0x60 (96)
    "LRDC", # 0x61 (97)
    "LRDB", # 0x62 (98)
    "LRUDT", # 0x63 (99)
    "LRU", # 0x64 (100)
    "LRUT" , # 0x65 (101)
    "LRUDT", # 0x66 (102)
    "LRUTX", # 0x67 (103)
    "LRUTX", # 0x68 (104)
    "LR", # 0x69 (105)
    "LUDC", # 0x6A (106)
    "RC", # 0x6B (107)
    "LRUB", # 0x6C (108)
    "LRUX", # 0x6D (109)
    "LRU", # 0x6E (110)
    "LC",  # 0x6F (111)

    "RUCX", # 0x70 (112) 
    "LRUB", # 0x71 (113)
    "LRUC", # 0x72 (114)
    "LRUT", # 0x73 (115)
    "LRP", # 0x74 (116)
    "LRUB", # 0x75 (117)
    "LRB", # 0x76 (118)
    "LR", # 0x77 (119)
    "LU", # 0x78 (120)
    "D", # 0x79 (121)
    "D", # 0x7A (122)
    "D", # 0x7B (123)
]

for s in range(0, len(screen_data)):
    exits = []
    caves = []
    meta = []
    sd = screen_data[s]    

    print(f"Screen {s:02X}: {sd}")

    output =  f"name: Screen {s:02X}\n"
    output += f"area: Overworld\n"
    output += f"screen: 0x{s:02X}\n"
    output += "nodes:\n"
    output += "  exits:\n"
    if "L" in sd:
        output += "    - name: \"Left exit\"\n"
        output += "      type: Scroll\n"
        output += "      direction: Left\n"
        output += "      \n"
        exits.append("Left exit")
    if "R" in sd:
        output += "    - name: \"Right exit\"\n"
        output += "      type: Scroll\n"
        output += "      direction: Right\n"
        output += "      \n"
        exits.append("Right exit")
    if "U" in sd:
        output += "    - name: \"Up exit\"\n"
        output += "      type: Scroll\n"
        output += "      direction: Up\n"
        output += "      \n"
        exits.append("Up exit")
    if "D" in sd:
        output += "    - name: \"Down exit\"\n"
        output += "      type: Scroll\n"
        output += "      direction: Down\n"
        output += "      \n"
        exits.append("Down exit")

    if "B" in sd or "C" in sd or "G" in sd or "T" in sd or "P" in sd:
        output += "  caves:\n"
        if "B" in sd:
            output += "    - name: \"Bomb cave\"\n"
            output += "      type: Bomb\n"
            output += "      \n"
            caves.append("Bomb cave")
        if "C" in sd:
            output += "    - name: \"Open cave\"\n"
            output += "      type: Open\n"
            output += "      \n"
            caves.append("Open cave")
        if "G" in sd:
            output += "    - name: \"Grave cave\"\n"
            output += "      type: Grave\n"
            output += "      \n"
            caves.append("Grave cave")
        if "T" in sd:
            output += "    - name: \"Tree cave\"\n"
            output += "      type: Tree\n"
            output += "      \n"
            caves.append("Tree cave")
        if "P" in sd:
            output += "    - name: \"Push block cave\"\n"
            output += "      type: Push\n"
            output += "      \n"
            caves.append("Push block cave")

    if "I" in sd or "A" in sd or "F" in sd:
        output += "  meta:\n"
        if "I" in sd:
            output += "    - name: \"Overworld Item\"\n"
            output += "      type: Item\n"
            output += "      \n"
            meta.append("Overworld Item")
        if "A" in sd:
            output += "    - name: \"Armos Knight\"\n"
            output += "      type: Armos\n"
            output += "      \n"
            meta.append("Armos Knight")
        if "F" in sd:
            output += "    - name: \"Fairy Fountain\"\n"
            output += "      type: Fairy\n"
            output += "      \n"
            meta.append("Fairy Fountain")


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

    # since we assume that open caves can always be accessed from one entrance,
    # we add a fixed connection to open caves
    for c in caves:
        if "Open" in c:
            output += f"      - [\"{c}\", \"{exits[0]}\"]\n"
    
    # same with meta nodes, just make them always accessible
    for m in meta:
        output += f"      - [\"{m}\", \"{exits[0]}\"]\n"

    # For other caves, we need to take care of what item we need to access it
    for c in [c for c in caves if "Bomb" in c]:
        output += f"    UseBombs:\n"
        output += f"      - [\"{c}\", \"{exits[0]}\"]\n"
    
    for c in [c for c in caves if "Push" in c]:
        output += f"    PushBlocks:\n"
        output += f"      - [\"{c}\", \"{exits[0]}\"]\n"

    for c in [c for c in caves if "Grave" in c]:
        output += f"    OpenGraves:\n"
        output += f"      - [\"{c}\", \"{exits[0]}\"]\n"

    for c in [c for c in caves if "Tree" in c]:
        output += f"    BurnTrees:\n"
        output += f"      - [\"{c}\", \"{exits[0]}\"]\n"

    #f = open(f"Screens/Overworld/{s:02X}.yml", "w")
    #f.write(output)
    #f.close()
    
