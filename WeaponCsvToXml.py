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
        super(WeaponImporter, self).__init__("Weapon Data V5", "WeaponClasses")
        write_tag(self, "Weapons", self.read_weapon_classes)

    def write_weapon_stats(self, row):
        write_single_value(self, "Damage", get_value(self, "D", row))
        write_single_value(self, "FireRate", get_value(self, "E", row))
        write_single_value(self, "ReloadSpeed", get_value(self, "F", row))
        write_single_value(self, "Accuracy", get_value(self, "G", row))
        write_single_value(self, "Handling", get_value(self, "H", row))
        write_single_value(self, "Capacity", get_value(self, "I", row))

    def read_weapon_subtypes(self, subtype_row):
        for i in range(0, 3):
            subtype_name = get_value(self, "C", subtype_row + i)
            automatic = get_value(self, "B", subtype_row + i)
            write_tag(self, "Subtype", self.write_weapon_stats, [subtype_row + i], ["name", "automatic"],
                      [subtype_name, automatic])

    def read_weapon_classes(self):
        for row_no in range(1, 6):
            weapon_class = get_value(self, "A", row_no * 3)
            write_tag(self, "Class", self.read_weapon_subtypes, [row_no * 3], ["name"],
                      [weapon_class])


class RecipeImporter(XMLWriter):
    def __init__(self):
        super(RecipeImporter, self).__init__("Recipes", "Recipes")
        write_tag(self, "Recipes", self.read_recipes)

    def read_recipe(self, row):
        write_single_value(self, "Ingredient1Name", get_value(self, "A", row, "None"))
        write_single_value(self, "Ingredient1Quantity", get_value(self, "B", row, "0"))
        write_single_value(self, "Ingredient2Name", get_value(self, "C", row, "None"))
        write_single_value(self, "Ingredient2Quantity", get_value(self, "D", row, "0"))
        write_single_value(self, "ProductName", get_value(self, "E", row, ""))
        write_single_value(self, "ProductQuantity", get_value(self, "F", row, "0"))

    def read_recipes(self):
        for row_no in range(2, 20):
            write_tag(self, "Recipe", self.read_recipe, [row_no])


class ResourceImporter(XMLWriter):
    def __init__(self):
        super(ResourceImporter, self).__init__("Resources", "Resources")
        write_tag(self, "Resources", self.read_resources)

    def read_resource(self, row, consumable):
        write_single_value(self, "Name", get_value(self, "A", row, ""))
        write_single_value(self, "Type", get_value(self, "B", row, ""))
        write_single_value(self, "OasisDropRate", get_value(self, "G", row, ""))
        write_single_value(self, "SteppeDropRate", get_value(self, "H", row, ""))
        write_single_value(self, "RuinsDropRate", get_value(self, "I", row, ""))
        write_single_value(self, "DefilesDropRate", get_value(self, "J", row, ""))
        write_single_value(self, "WastelandDropRate", get_value(self, "K", row, ""))
        if consumable:
            write_single_value(self, "Attribute", get_value(self, "C", row, ""))
            write_single_value(self, "Modifier", get_value(self, "D", row, ""))
            write_single_value(self, "Bonus", get_value(self, "E", row, ""))
            write_single_value(self, "Duration", get_value(self, "F", row, ""))

    def read_resources(self):
        for row_no in range(3, 34):
            if get_value(self, "B", row_no) == "Resource":
                write_tag(self, "Resource", self.read_resource, [row_no, False])
            else:
                write_tag(self, "Consumable", self.read_resource, [row_no, True])


class InscriptionImporter(XMLWriter):
    def __init__(self):
        super(InscriptionImporter, self).__init__("Inscriptions", "Inscriptions")
        write_tag(self, "Inscriptions", self.read_inscriptions)

    def read_inscriptions(self):
        for row in range(2, 16):
            write_tag(self, "Inscription", self.read_inscription, [row])

    def read_inscription(self, row):
        write_single_value(self, "Name", get_value(self, "A", row))
        write_single_value(self, "Attribute", get_value(self, "B", row))
        write_single_value(self, "Value", get_value(self, "C", row))
        write_single_value(self, "Additive", get_value(self, "D", row))


class GearImporter(XMLWriter):
    def __init__(self):
        super(GearImporter, self).__init__("Gear", "Gear")
        write_tag(self, "GearList", self.read_gear)

    def read_gear(self):
        for row in range(3, 13):
            write_tag(self, "Gear", self.read_single_gear, [row])

    def read_single_gear(self, row):
        write_single_value(self, "Name", get_value(self, "A", row))
        write_single_value(self, "Attribute", get_value(self, "B", row))
        write_single_value(self, "Bonus", get_value(self, "C", row))


class WeatherImporter(XMLWriter):
    def __init__(self):
        super(WeatherImporter, self).__init__("Weather", "Weather")
        write_tag(self, "WeatherTypes", self.read_weather)

    def read_weather(self):
        for row in range(3, 19):
            write_tag(self, "Weather", self.read_single_weather, [row])

    def read_single_weather(self, row):
        write_single_value(self, "Name", get_value(self, "A", row, "0"))
        write_single_value(self, "DisplayName", get_value(self, "B", row, "0"))
        write_single_value(self, "Temperature", get_value(self, "C", row, "0"))
        write_single_value(self, "Visibility", get_value(self, "D", row, "0"))
        write_single_value(self, "Water", get_value(self, "E", row, "0"))
        write_single_value(self, "Fog", get_value(self, "F", row, "0"))
        write_single_value(self, "Ice", get_value(self, "G", row, "0"))
        write_single_value(self, "Duration", get_value(self, "H", row, "0"))
        write_single_value(self, "Thunder", get_value(self, "I", row, "0"))
        write_tag(self, "Particles", self.read_particles, [row])

    def read_particles(self, row):
        write_single_value(self, "Rain", get_value(self, "J", row, "0"))
        write_single_value(self, "Fog", get_value(self, "K", row, "0"))
        write_single_value(self, "Dust", get_value(self, "L", row, "0"))
        write_single_value(self, "Hail", get_value(self, "M", row, "0"))
        write_single_value(self, "Wind", get_value(self, "N", row, "0"))
        write_single_value(self, "Sun", get_value(self, "O", row, "0"))


class EnvironmentImporter(XMLWriter):
    def __init__(self):
        super(EnvironmentImporter, self).__init__("Environments", "Environments")
        write_tag(self, "EnvironmentTypes", self.read_environment)

    def read_environment(self):
        for row in range(3, 8):
            write_tag(self, get_value(self, "A", row), self.read_single_environment, [row])

    def read_single_environment(self, row):
        write_single_value(self, "Level", get_value(self, "B", row))
        write_single_value(self, "Temperature", get_value(self, "C", row))
        write_single_value(self, "Temples", get_value(self, "D", row))
        write_single_value(self, "Monuments", get_value(self, "E", row))
        write_single_value(self, "Shrines", get_value(self, "F", row))
        write_single_value(self, "Fountains", get_value(self, "G", row))
        write_single_value(self, "Shelters", get_value(self, "H", row))
        write_single_value(self, "Animals", get_value(self, "I", row))
        write_single_value(self, "Danger", get_value(self, "J", row))


class RegionImporter(XMLWriter):
    def __init__(self):
        super(RegionImporter, self).__init__("Regions", "Regions")
        write_tag(self, "Names", self.read_names)

    def read_names(self):
        write_tag(self, "RegionType", self.read_regions)
        write_tag(self, "EnvironmentSuffixes", self.read_environment_suffixes)

    def read_regions(self):
        for offset in range(0, 7):
            column = offset * 2 + 1
            column_letter = num2alpha[column]
            write_tag(self, get_value(self, column_letter, 1), self.read_region_type, [column])

    def read_environment_suffixes(self):
        self.read_environment_suffix("P")
        self.read_environment_suffix("Q")
        self.read_environment_suffix("R")
        self.read_environment_suffix("S")
        self.read_environment_suffix("T")

    def read_environment_suffix(self, column_letter):
        prefix_string = ""
        for row in range(3, 16):
            prefix = get_value(self, column_letter, row)
            if prefix == "1":
                break
            if prefix_string != "":
                prefix_string += ","
            prefix_string += prefix
        write_single_value(self, get_value(self, column_letter, 2), prefix_string)

    def read_region_type(self, column):
        self.read_region_names(num2alpha[column], "Prefixes")
        self.read_region_names(num2alpha[column + 1], "Suffixes")

    def read_region_names(self, column_letter, name_type):
        prefix_string = ""
        for row in range(3, 40):
            prefix = get_value(self, column_letter, row)
            if prefix == "1":
                break
            if prefix_string != "":
                prefix_string += ","
            prefix_string += prefix
        write_single_value(self, name_type, prefix_string)


class CharacterImporter(XMLWriter):
    def __init__(self):
        super(CharacterImporter, self).__init__("Classes", "Classes")
        write_tag(self, "Classes", self.read_classes)

    def read_classes(self):
        for row in range(3, 12):
            write_tag(self, "Class", self.read_class, [row])

    def read_class(self, row):
        write_single_value(self, "Name", get_value(self, "A", row))
        write_single_value(self, "Endurance", get_value(self, "D", row))
        write_single_value(self, "Strength", get_value(self, "E", row))
        write_single_value(self, "Willpower", get_value(self, "F", row))
        write_single_value(self, "Perception", get_value(self, "G", row))
        write_single_value(self, "Story", get_value(self, "M", row))


class EnemyImporter(XMLWriter):
    def __init__(self):
        super(EnemyImporter, self).__init__("Enemy Types", "Enemies")
        write_tag(self, "Enemies", self.read_enemies)

    def read_enemies(self):
        for row in range(3, 24):
            write_tag(self, "Enemy", self.read_enemy, [row])

    def read_enemy(self, row):
        write_single_value(self, "Name", get_value(self, "A", row))
        write_single_value(self, "Health", get_value(self, "B", row))
        write_single_value(self, "Speed", get_value(self, "D", row))
        write_single_value(self, "Value", get_value(self, "E", row))
        write_single_value(self, "Drops", get_value(self, "F", row, ""))
        write_single_value(self, "HasWeapon", get_value(self, "G", row))
        write_single_value(self, "HasGear", get_value(self, "H", row))
        write_single_value(self, "Species", get_value(self, "I", row))


class BrandImporter(XMLWriter):
    def __init__(self):
        super(BrandImporter, self).__init__("Unlockables", "Brands")
        write_tag(self, "Brands", self.read_brands)

    def read_brands(self):
        for row in range(3, 20):
            write_tag(self, get_value(self, "A", row), self.read_brand, [row])

    def read_brand(self, row):
        write_single_value(self, "Requirement", get_value(self, "B", row))
        write_single_value(self, "TargetValue", get_value(self, "C", row))
        write_single_value(self, "SuccessName", get_value(self, "D", row))
        write_single_value(self, "SuccessEffect", get_value(self, "E", row))
        write_single_value(self, "SuccessValue", get_value(self, "F", row, ""))
        write_single_value(self, "FailName", get_value(self, "G", row))
        write_single_value(self, "FailEffect", get_value(self, "H", row))
        write_single_value(self, "FailValue", get_value(self, "I", row))


class SkillImporter(XMLWriter):
    def __init__(self):
        super(SkillImporter, self).__init__("Skills", "Skills")
        write_tag(self, "Skills", self.read_skills)

    def read_skills(self):
        for row in range(2, 43):
            if (row - 1 % 3) == 0:
                continue
            write_tag(self, "Skill", self.read_skill, [row])

    def read_skill(self, row):
        write_single_value(self, "Name", get_value(self, "A", row))
        write_single_value(self, "RequiresTarget", get_value(self, "D", row))
        write_single_value(self, "AppliesToMagazine", get_value(self, "E", row))
        write_single_value(self, "Cooldown", get_value(self, "F", row))
        write_single_value(self, "Description", get_value(self, "G", row))


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


def get_value(xml_writer, column, row, default_value="1"):
    if column.isdigit():
        column = num2alpha[row]
    value = xml_writer.sheet[column + str(row)].value
    if value is None:
        return default_value
    return str(value)


def write_single_value(xml_writer, stat_name, value):
    xml_writer.output_file.writelines("<" + stat_name + ">" + value + "</" + stat_name + ">")


# WeaponImporter()
# GearImporter()
# WeatherImporter()
# RegionImporter()
# CharacterImporter()
# EnemyImporter()
# RecipeImporter()
# ResourceImporter()
# InscriptionImporter()
# SkillImporter()
# EnvironmentImporter()
BrandImporter()
