from openpyxl import load_workbook
import string

workbook = load_workbook('Desolation Balance Files.xlsx', data_only=True)
num2alpha = dict(zip(range(1, 27), string.ascii_lowercase))


class XMLWriter:
    def __init__(self, sheet_name, output_filename):
        self.sheet = workbook[sheet_name]
        self.output_file = open('Night Unity Files/Assets/Resources/XML/' + output_filename + ".xml", 'w')

    def __del__(self):
        self.output_file.close()


class WeaponImporter(XMLWriter):
    def __init__(self):
        super(WeaponImporter, self).__init__("Weapon Data V4", "WeaponClasses")
        write_tag(self, "Weapons", self.read)

    def write_weapon_stats(self, row):
        write_single_value(self, "Damage", "+" + get_value(self, "L", row))
        write_single_value(self, "Range", "+" + get_value(self, "M", row))
        write_single_value(self, "FireRate", "+" + get_value(self, "N", row))
        write_single_value(self, "ReloadSpeed", "+" + get_value(self, "O", row))
        write_single_value(self, "CriticalChance", "+" + get_value(self, "P", row))
        write_single_value(self, "Handling", "+" + get_value(self, "Q", row))
        write_single_value(self, "Capacity", "+" + get_value(self, "R", row))
        write_single_value(self, "Pellets", "+" + get_value(self, "S", row))

    def write_weapon_mod_stats(self, row):
        write_single_value(self, "Damage", "x" + get_value(self, "D", row))
        write_single_value(self, "Range", "x" + get_value(self, "E", row))
        write_single_value(self, "FireRate", "x" + get_value(self, "F", row))
        write_single_value(self, "ReloadSpeed", "x" + get_value(self, "G", row))
        write_single_value(self, "CriticalChance", "x" + get_value(self, "H", row))
        write_single_value(self, "Handling", "x" + get_value(self, "I", row))
        write_single_value(self, "Capacity", "x" + get_value(self, "J", row))
        write_single_value(self, "Pellets", "x" + get_value(self, "K", row))
        write_single_value(self, "PierceChance", "+" + get_value(self, "L", row, 0))
        write_single_value(self, "BleedChance", "+" + get_value(self, "M", row, 0))
        write_single_value(self, "BurnChance", "+" + get_value(self, "N", row, 0))
        write_single_value(self, "SicknessChance", "+" + get_value(self, "O", row, 0))

    def read_weapon_subtypes(self, subtype_row):
        for i in range(0, 3):
            subtype_name = get_value(self, "C", subtype_row + i)
            manual = get_value(self, "B", subtype_row + i)
            write_tag(self, "Subtype", self.write_weapon_stats, [subtype_row + i], ["name", "manual"], [subtype_name, manual])

    def read_weapon_modifiers(self):
        for i in range(27, 48):
            modifier_name = get_value(self, "B", i)
            write_tag(self, "Modifier", self.write_weapon_mod_stats, [i], ["name"], [modifier_name])

    def read_weapon_classes(self):
        for row_no in range(3, 8):
            weapon_class = get_value(self, "A", row_no)
            magazine_cost = get_value(self, "N", row_no)
            write_tag(self, "Class", self.read_weapon_subtypes, [row_no * 3 + 2], ["name", "ammoCost"], [weapon_class, magazine_cost])

    def read(self):
        write_tag(self, "Classes", self.read_weapon_classes)
        write_tag(self, "Modifiers", self.read_weapon_modifiers)


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
        write_single_value(self, "Weight", "+" + get_value(self, "D", row))
        write_single_value(self, "Description", get_value(self, "E", row))
        if gear_type == "Accessory":
            write_single_value(self, "Effect", get_value(self, "F", row) + get_value(self, "G", row))
        else:
            write_single_value(self, "Armour", "+" + get_value(self, "H", row, 0))
            write_single_value(self, "Perception", "+" + get_value(self, "I", row, 0))
            write_single_value(self, "Willpower", "+" + get_value(self, "J", row, 0))
            write_single_value(self, "Strength", "+" + get_value(self, "K", row, 0))
            write_single_value(self, "Endurance", "+" + get_value(self, "L", row, 0))


class WeatherImporter(XMLWriter):
    def __init__(self):
        super(WeatherImporter, self).__init__("Weather", "Weather")
        write_tag(self, "WeatherTypes", self.read_weather)

    def read_weather(self):
        for row in range(3, 24):
            write_tag(self, "Weather", self.read_single_weather, [row])

    def read_single_weather(self, row):
        write_single_value(self, "Name", get_value(self, "A", row, 0))
        write_single_value(self, "Type", get_value(self, "C", row, 0))
        write_single_value(self, "Temperature", get_value(self, "D", row, 0))
        write_single_value(self, "Visibility", get_value(self, "E", row, 0))
        write_single_value(self, "Water", get_value(self, "F", row, 0))
        write_single_value(self, "Food", get_value(self, "G", row, 0))
        write_single_value(self, "Duration", get_value(self, "H", row, 0))
        write_tag(self, "Particles", self.read_particle_values, [row])

    def read_particle_values(self, row):
        write_single_value(self, "Rain", get_value(self, "I", row, 0))
        write_single_value(self, "Fog", get_value(self, "J", row, 0))
        write_single_value(self, "Dust", get_value(self, "K", row, 0))
        write_single_value(self, "Hail", get_value(self, "L", row, 0))
        write_single_value(self, "Sun", get_value(self, "M", row, 0))


class EnvironmentImporter(XMLWriter):
    def __init__(self):
        super(EnvironmentImporter, self).__init__("Environments", "Environments")
        write_tag(self, "EnvironmentTypes", self.read_environment)

    def read_environment(self):
        for row in range(3, 8):
            write_tag(self, get_value(self, "A", row), self.read_single_environment, [row])

    def read_single_environment(self, row):
        write_single_value(self, "Temperature", get_value(self, "B", row))
        write_single_value(self, "Wetness", get_value(self, "C", row))
        write_single_value(self, "Water", get_value(self, "D", row))
        write_single_value(self, "Food", get_value(self, "E", row))
        write_single_value(self, "Fuel", get_value(self, "F", row))
        write_single_value(self, "Scrap", get_value(self, "G", row))


class RegionImporter(XMLWriter):
    def __init__(self):
        super(RegionImporter, self).__init__("Regions", "Regions")
        write_tag(self, "RegionType", self.read_regions)

    def read_regions(self):
        for offset in range(0, 5):
            column = 2 + offset * 3
            column_letter = num2alpha[column]
            write_tag(self, get_value(self, column_letter, 2), self.read_region_type, [column])

    def read_region_type(self, column):
        self.read_region_names(num2alpha[column], "Prefixes")
        self.read_region_names(num2alpha[column + 1], "Suffixes")
        write_tag(self, "Region", self.read_single_region, [column])
        write_tag(self, "Region", self.read_single_region, [column + 1])
        write_tag(self, "Region", self.read_single_region, [column + 2])

    def read_region_names(self, column_letter, name_type):
        prefix_string = ""
        for row in range(11, 38):
            prefix = get_value(self, column_letter, row)
            if prefix == "1":
                break
            if prefix_string != "":
                prefix_string += ","
            prefix_string += prefix
        write_single_value(self, name_type, prefix_string)

    def read_single_region(self, column):
        column_letter = num2alpha[column]
        write_single_value(self, "Name", get_value(self, column_letter, 1))
        write_single_value(self, "Type", get_value(self, column_letter, 2))
        write_single_value(self, "Food", get_value(self, column_letter, 3))
        write_single_value(self, "Water", get_value(self, column_letter, 4))
        write_single_value(self, "Fuel", get_value(self, column_letter, 5))
        write_single_value(self, "Scrap", get_value(self, column_letter, 6))
        write_single_value(self, "Ammo", get_value(self, column_letter, 7))


class CharacterImporter(XMLWriter):
    def __init__(self):
        super(CharacterImporter, self).__init__("Classes", "Classes")
        write_tag(self, "Classes", self.read_classes)

    def read_classes(self):
        for row in range(3, 13):
            write_tag(self, "Class", self.read_class, [row])

    def read_class(self, row):
        write_single_value(self, "Name", get_value(self, "A", row))
        write_single_value(self, "Endurance", get_value(self, "D", row))
        write_single_value(self, "Strength", get_value(self, "E", row))
        write_single_value(self, "Willpower", get_value(self, "F", row))
        write_single_value(self, "Perception", get_value(self, "G", row))
        write_single_value(self, "Story", get_value(self, "N", row))


class EnemyImporter(XMLWriter):
    def __init__(self):
        super(EnemyImporter, self).__init__("Enemy Types", "Enemies")
        write_tag(self, "Enemies", self.read_enemies)

    def read_enemies(self):
        for row in range(2, 10):
            write_tag(self, "Enemy", self.read_enemy, [row])

    def read_enemy(self, row):
        write_single_value(self, "Name", get_value(self, "A", row))
        write_single_value(self, "Health", get_value(self, "B", row))
        write_single_value(self, "Speed", get_value(self, "C", row))
        write_single_value(self, "Value", get_value(self, "D", row))
        write_single_value(self, "WeaponTypes", get_value(self, "E", row))


class SkillImporter(XMLWriter):
    def __init__(self):
        super(SkillImporter, self).__init__("Skills", "Skills")
        write_tag(self, "Skills", self.read_skills)

    def read_skills(self):
        for row in range(2, 46):
            if (row - 1 % 3) == 0:
                continue
            write_tag(self, "Skill", self.read_skill, [row])

    def read_skill(self, row):
        write_single_value(self, "Name", get_value(self, "A", row))
        write_single_value(self, "Cost", get_value(self, "C", row))
        write_single_value(self, "Cooldown", get_value(self, "D", row))
        write_single_value(self, "Description", get_value(self, "E", row))


class TraitImporter(XMLWriter):
    def __init__(self):
        super(TraitImporter, self).__init__("Traits", "Traits")
        write_tag(self, "Traits", self.read_traits)

    def read_traits(self):
        for row in range(2, 47):
            write_tag(self, "Skill", self.read_trait, [row])

    def read_trait(self, row):
        write_single_value(self, "Name", get_value(self, "A", row))
        write_single_value(self, "Requirement", get_value(self, "C", row))


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
        return str(default_value)
    return str(value)


def write_single_value(xml_writer, stat_name, value):
    xml_writer.output_file.writelines("<" + stat_name + ">" + value + "</" + stat_name + ">")


# WeaponImporter()
# GearImporter()
# WeatherImporter()
# RegionImporter()
# CharacterImporter()
EnemyImporter()
# SkillImporter()
# TraitImporter()
