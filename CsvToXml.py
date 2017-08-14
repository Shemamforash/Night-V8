csv_file = open('WeaponBalance.csv')
xml_file = open('Night Unity Files/Assets/Resources/WeaponClasses.xml', 'w')

xml_file.writelines("<WeaponClasses>")
ctr = 0


def writeweaponattributes(weapon_attributes):
    xml_file.writelines("<Suffix>" + weapon_attributes[0] + "</Suffix>")
    xml_file.writelines("<BaseStats>")
    xml_file.writelines("<Damage>" + weapon_attributes[1] + "-" + weapon_attributes[2] + "</Damage>")
    xml_file.writelines("<Accuracy>" + weapon_attributes[3] + "-" + weapon_attributes[4] + "</Accuracy>")
    xml_file.writelines("<FireRate>" + weapon_attributes[5] + "-" + weapon_attributes[6] + "</FireRate>")
    xml_file.writelines("<Handling>" + weapon_attributes[7] + "-" + weapon_attributes[8] + "</Handling>")
    xml_file.writelines("<ReloadSpeed>" + weapon_attributes[9] + "-" + weapon_attributes[10] + "</ReloadSpeed>")
    xml_file.writelines("<Capacity>" + weapon_attributes[11] + "-" + weapon_attributes[12] + "</Capacity>")
    xml_file.writelines("<CriticalChance>" + weapon_attributes[13] + "-" + weapon_attributes[14] + "</CriticalChance>")
    xml_file.writelines("<NoPellets>" + weapon_attributes[15] + "</NoPellets>")
    xml_file.writelines("</BaseStats>")


for line in csv_file:
    line = line.strip("\n")
    line_contents = line.split(",")
    if line_contents[0] == "":
        continue
    weapon_class = line_contents[0]
    manual_allowed = str(line_contents[1] == "True")
    del line_contents[0]
    del line_contents[0]
    if ctr == 0:
        xml_file.writelines("<Class name=\"" + weapon_class + "\" manualAllowed=\"" + manual_allowed + "\">")
        xml_file.writelines("<Rusty>")
        writeweaponattributes(line_contents)
        xml_file.writelines("</Rusty>")
    elif ctr == 1:
        xml_file.writelines("<Tarnished>")
        writeweaponattributes(line_contents)
        xml_file.writelines("</Tarnished>")
    elif ctr == 2:
        xml_file.writelines("<Shiny>")
        writeweaponattributes(line_contents)
        xml_file.writelines("</Shiny>")
    elif ctr == 3:
        xml_file.writelines("<Gleaming>")
        writeweaponattributes(line_contents)
        xml_file.writelines("</Gleaming>")
    elif ctr == 4:
        xml_file.writelines("<Radiant>")
        writeweaponattributes(line_contents)
        xml_file.writelines("</Radiant>")
        xml_file.writelines("</Class>")
        ctr = -1
    ctr += 1


xml_file.writelines("</WeaponClasses>")
csv_file.close()
xml_file.close()

