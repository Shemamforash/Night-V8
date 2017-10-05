from openpyxl import load_workbook
import string

weapon_sheet = load_workbook('Desolation Balance Files.xlsx')["New Weapon Data"]
xml_file = open('Night Unity Files/Assets/Resources/WeaponClasses.xml', 'w')
num2alpha = dict(zip(range(1, 27), string.ascii_lowercase))


def get_value(column, row):
    if column.isdigit():
        column = num2alpha[row]
    return weapon_sheet[column + str(row)].value


def write_subtype_stat(stat_name, value):
    xml_file.writelines("<" + stat_name + ">" + str(value) + "</" + stat_name + ">")


def read_weapon_subtypes(subtype_row):
    for i in range(0, 5):
        subtype_name = get_value("B", subtype_row)
        xml_file.writelines("<Subtype name=\"" + subtype_name + "\">")
        write_subtype_stat("Damage", get_value("D", subtype_row))
        write_subtype_stat("Accuracy", get_value("E", subtype_row))
        write_subtype_stat("FireRate", get_value("F", subtype_row))
        write_subtype_stat("Handling", get_value("G", subtype_row))
        write_subtype_stat("ReloadSpeed", get_value("H", subtype_row))
        write_subtype_stat("CriticalChance", get_value("I", subtype_row))
        write_subtype_stat("Capacity", get_value("J", subtype_row))
        write_subtype_stat("Pellets", get_value("K", subtype_row))
        xml_file.writelines("</Subtype>")
        subtype_row += 1


def read_weapon_classes():
    xml_file.writelines("<WeaponClasses>")
    for i in range(1, 6):
        row_no = i * 3
        weapon_class = get_value("A", row_no)
        manual_allowed = str(get_value("B", row_no))
        xml_file.writelines("<Class name=\"" + weapon_class + "\" manualAllowed=\"" + manual_allowed + "\">")
        xml_file.writelines("<BaseStats>")
        write_stat("Damage", get_value("E", row_no), get_value("F", row_no))
        write_stat("Accuracy", get_value("I", row_no), get_value("J", row_no))
        write_stat("FireRate", get_value("M", row_no), get_value("N", row_no))
        write_stat("Handling", get_value("Q", row_no), get_value("R", row_no))
        write_stat("ReloadSpeed", get_value("U", row_no), get_value("V", row_no))
        write_stat("CriticalChance", get_value("Y", row_no), get_value("Z", row_no))
        xml_file.writelines("</BaseStats>")
        read_weapon_subtypes(i * 5 + 16)
        xml_file.writelines("</Class>")
    xml_file.writelines("</WeaponClasses>")
    xml_file.close()
    exit()


def write_stat(stat_name, x_coefficient, intercept):
    xml_file.writelines("<" + stat_name + ">")
    xml_file.writelines("<XCoefficient>" + str(x_coefficient) + "</XCoefficient>")
    xml_file.writelines("<Intercept>" + str(intercept) + "</Intercept>")
    xml_file.writelines("</" + stat_name + ">")

read_weapon_classes()

# ctr = 0
#
#
# def writeweaponattributes(weapon_attributes):
#     xml_file.writelines("<Suffix>" + weapon_attributes[0] + "</Suffix>")
#     xml_file.writelines("<BaseStats>")
#     xml_file.writelines("<Damage>" + weapon_attributes[1] + "-" + weapon_attributes[2] + "</Damage>")
#     xml_file.writelines("<Accuracy>" + weapon_attributes[3] + "-" + weapon_attributes[4] + "</Accuracy>")
#     xml_file.writelines("<FireRate>" + weapon_attributes[5] + "-" + weapon_attributes[6] + "</FireRate>")
#     xml_file.writelines("<Handling>" + weapon_attributes[7] + "-" + weapon_attributes[8] + "</Handling>")
#     xml_file.writelines("<ReloadSpeed>" + weapon_attributes[9] + "-" + weapon_attributes[10] + "</ReloadSpeed>")
#     xml_file.writelines("<Capacity>" + weapon_attributes[11] + "-" + weapon_attributes[12] + "</Capacity>")
#     xml_file.writelines("<CriticalChance>" + weapon_attributes[13] + "-" + weapon_attributes[14] + "</CriticalChance>")
#     xml_file.writelines("<NoPellets>" + weapon_attributes[15] + "</NoPellets>")
#     xml_file.writelines("</BaseStats>")
#
#
# for line in csv_file:
#     line = line.strip("\n")
#     line_contents = line.split(",")
#     if line_contents[0] == "":
#         continue
#     weapon_class = line_contents[0]
#     manual_allowed = str(line_contents[1] == "True")
#     del line_contents[0]
#     del line_contents[0]
#     if ctr == 0:
#         xml_file.writelines("<Class name=\"" + weapon_class + "\" manualAllowed=\"" + manual_allowed + "\">")
#         xml_file.writelines("<Rusty>")
#         writeweaponattributes(line_contents)
#         xml_file.writelines("</Rusty>")
#     elif ctr == 1:
#         xml_file.writelines("<Tarnished>")
#         writeweaponattributes(line_contents)
#         xml_file.writelines("</Tarnished>")
#     elif ctr == 2:
#         xml_file.writelines("<Shiny>")
#         writeweaponattributes(line_contents)
#         xml_file.writelines("</Shiny>")
#     elif ctr == 3:
#         xml_file.writelines("<Gleaming>")
#         writeweaponattributes(line_contents)
#         xml_file.writelines("</Gleaming>")
#     elif ctr == 4:
#         xml_file.writelines("<Radiant>")
#         writeweaponattributes(line_contents)
#         xml_file.writelines("</Radiant>")
#         xml_file.writelines("</Class>")
#         ctr = -1
#     ctr += 1
