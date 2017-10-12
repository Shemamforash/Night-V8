from openpyxl import load_workbook
import string

workbook = load_workbook('Desolation Balance Files.xlsx')
num2alpha = dict(zip(range(1, 27), string.ascii_lowercase))


class XMLWriter:
    def __init__(self, sheet_name, output_filename):
        self.sheet = workbook[sheet_name]
        self.output_file = open('Night Unity Files/Assets/Resources/' + output_filename + ".xml", 'w')

    def __del__(self):
        self.output_file.close()


class WeaponImporter(XMLWriter):
    def __init__(self):
        super(WeaponImporter, self).__init__("New Weapon Data", "WeaponClasses")
        write_tag(self, "Weapons", self.read)

    def write_weapon_stats(self, row):
        write_single_value(self, "Damage", get_value(self, "D", row))
        write_single_value(self, "Accuracy", get_value(self, "E", row))
        write_single_value(self, "FireRate", get_value(self, "F", row))
        write_single_value(self, "Handling", get_value(self, "G", row))
        write_single_value(self, "ReloadSpeed", get_value(self, "H", row))
        write_single_value(self, "CriticalChance", get_value(self, "I", row))
        write_single_value(self, "Capacity", get_value(self, "J", row))
        write_single_value(self, "Pellets", get_value(self, "K", row))

    def read_weapon_subtypes(self, subtype_row):
        for i in range(0, 5):
            subtype_name = get_value(self, "B", subtype_row)
            write_tag(self, "Subtype", self.write_weapon_stats, [subtype_row], ["name"], [subtype_name])
            subtype_row += 1

    def read_weapon_modifiers(self):
        for i in range(46, 59):
            modifier_name = get_value(self, "B", i)
            write_tag(self, "Modifier", self.write_weapon_stats, [i], ["name"], [modifier_name])

    def read_weapon_classes(self):
        for i in range(1, 6):
            row_no = i * 3
            weapon_class = get_value(self, "A", row_no)
            manual_allowed = str(get_value(self, "B", row_no))
            write_tag(self, "Class", self.read_single_class, [row_no, i], ["name", "manualAllowed"], [weapon_class, manual_allowed])

    def read_single_class(self, row_no, i):
        write_tag(self, "BaseStats", self.write_base_stats, [row_no])
        self.read_weapon_subtypes(i * 5 + 16)

    def write_base_stats(self, row_no):
        write_tag(self, "Damage", self.write_stat_law, ["E", "F", row_no])
        write_tag(self, "Accuracy", self.write_stat_law, ["I", "J", row_no])
        write_tag(self, "FireRate", self.write_stat_law, ["M", "N", row_no])
        write_tag(self, "Handling", self.write_stat_law, ["Q", "R", row_no])
        write_tag(self, "ReloadSpeed", self.write_stat_law, ["U", "V", row_no])
        write_tag(self, "CriticalChance", self.write_stat_law, ["Y", "Z", row_no])

    def read(self):
        write_tag(self, "Classes", self.read_weapon_classes)
        write_tag(self, "Modifiers", self.read_weapon_modifiers)

    def write_stat_law(self, coefficient_column, intercept_column, row):
        x_coefficient = get_value(self, coefficient_column, row)
        intercept = get_value(self, intercept_column, row)
        write_single_value(self, "XCoefficient", str(x_coefficient))
        write_single_value(self, "Intercept", str(intercept))


class GearImporter(XMLWriter):
    def __init__(self):
        super(GearImporter, self).__init__("Gear", "Gear")
        write_tag(self, "GearList", self.read_gear)

    def read_gear(self):
        for row in range(3, 19):
            name = get_value(self, "B", row)
            if name is 1:
                name = get_value(self, "A", row)
            gear_type = get_value(self, "C", row)
            write_tag(self, gear_type, self.read_single_gear, [name, gear_type, row])

    def read_single_gear(self, name, gear_type, row):
        write_single_value(self, "Name", name)
        write_single_value(self, "Weight", get_value(self, "D", row))
        write_single_value(self, "Description", get_value(self, "E", row))
        if gear_type == "Accessory":
            write_single_value(self, "Effect", get_value(self, "F", row) + get_value(self, "G", row))
        else:
            write_single_value(self, "Armour", get_value(self, "H", row, 0))
            write_single_value(self, "Intelligence", get_value(self, "I", row, 0))
            write_single_value(self, "Stability", get_value(self, "J", row, 0))
            write_single_value(self, "Strength", get_value(self, "K", row, 0))
            write_single_value(self, "Endurance", get_value(self, "L", row, 0))


def write_tag(xml_writer, tag_name, nested_method=None, args=None, parameters=[], values=[]):
    tag = "<" + tag_name
    for parameter_value in zip(parameters, values):
        tag += " " + parameter_value[0] + "=\"" + parameter_value[1] + "\" "
    tag += ">"
    xml_writer.output_file.writelines(tag)
    if nested_method is not None:
        if args is not None:
            nested_method(*args)
        else:
            nested_method()
    xml_writer.output_file.writelines("</" + tag_name + ">")


def get_value(xml_writer, column, row, default_value=1):
    if column.isdigit():
        column = num2alpha[row]
    value = xml_writer.sheet[column + str(row)].value
    if value is None:
        return default_value
    return value


def write_single_value(xml_writer, stat_name, value):
    xml_writer.output_file.writelines("<" + stat_name + ">" + str(value) + "</" + stat_name + ">")


WeaponImporter()
GearImporter()
