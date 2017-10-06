from openpyxl import load_workbook
import string

weapon_sheet = load_workbook('Desolation Balance Files.xlsx')["New Weapon Data"]
xml_file = open('Night Unity Files/Assets/Resources/WeaponClasses.xml', 'w')
num2alpha = dict(zip(range(1, 27), string.ascii_lowercase))


def get_value(column, row, default_value=1):
    if column.isdigit():
        column = num2alpha[row]
    value = weapon_sheet[column + str(row)].value
    if value is None:
        return default_value
    return value


def write_subtype_stat(stat_name, value):
    xml_file.writelines("<" + stat_name + ">" + str(value) + "</" + stat_name + ">")


def read_weapon_subtypes(subtype_row):
    for i in range(0, 5):
        subtype_name = get_value("B", subtype_row)
        xml_file.writelines("<Subtype name=\"" + subtype_name + "\">")
        write_stat(subtype_row)
        xml_file.writelines("</Subtype>")
        subtype_row += 1


def write_stat(row):
    write_subtype_stat("Damage", get_value("D", row))
    write_subtype_stat("Accuracy", get_value("E", row))
    write_subtype_stat("FireRate", get_value("F", row))
    write_subtype_stat("Handling", get_value("G", row))
    write_subtype_stat("ReloadSpeed", get_value("H", row))
    write_subtype_stat("CriticalChance", get_value("I", row))
    write_subtype_stat("Capacity", get_value("J", row))
    write_subtype_stat("Pellets", get_value("K", row))


def read_weapon_modifiers():
    xml_file.writelines("<Modifiers>")
    for i in range(46, 59):
        modifier_name = get_value("B", i)
        xml_file.writelines("<Modifier name=\"" + modifier_name + "\">")
        write_stat(i)
        xml_file.writelines("</Modifier>")
    xml_file.writelines("</Modifiers>")


def read_weapon_classes():
    xml_file.writelines("<Weapons>")
    xml_file.writelines("<Classes>")
    for i in range(1, 6):
        row_no = i * 3
        weapon_class = get_value("A", row_no)
        manual_allowed = str(get_value("B", row_no))
        xml_file.writelines("<Class name=\"" + weapon_class + "\" manualAllowed=\"" + manual_allowed + "\">")
        xml_file.writelines("<BaseStats>")
        write_stat_law("Damage", get_value("E", row_no), get_value("F", row_no))
        write_stat_law("Accuracy", get_value("I", row_no), get_value("J", row_no))
        write_stat_law("FireRate", get_value("M", row_no), get_value("N", row_no))
        write_stat_law("Handling", get_value("Q", row_no), get_value("R", row_no))
        write_stat_law("ReloadSpeed", get_value("U", row_no), get_value("V", row_no))
        write_stat_law("CriticalChance", get_value("Y", row_no), get_value("Z", row_no))
        xml_file.writelines("</BaseStats>")
        read_weapon_subtypes(i * 5 + 16)
        xml_file.writelines("</Class>")
    xml_file.writelines("</Classes>")
    read_weapon_modifiers()
    xml_file.writelines("</Weapons>")
    xml_file.close()
    exit()


def write_stat_law(stat_name, x_coefficient, intercept):
    xml_file.writelines("<" + stat_name + ">")
    xml_file.writelines("<XCoefficient>" + str(x_coefficient) + "</XCoefficient>")
    xml_file.writelines("<Intercept>" + str(intercept) + "</Intercept>")
    xml_file.writelines("</" + stat_name + ">")

read_weapon_classes()