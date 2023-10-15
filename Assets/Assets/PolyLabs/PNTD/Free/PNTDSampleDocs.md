# PNTD Free Substances & Materials Docs

 **PolyLabs | version 1.0.0** | support@polylabs.co

## Package Overview

This package contains Substance3D materials *and* Unity materials. Substances can be used in Unity by installing the **Substance3D for Unity** package on the Unity Asset Store.

This documentation is also a reference guide for modifying the Susbtances via script, with the parameters and their ranges.

> Substances are procedural materials that allow developers to change the look and feel of materials at runtime, or even give them the ability to quickly customize materials before statically rendering the bitmaps.
>
> At the time of writing this documentation, the plugin is currently only in beta, so please take caution installing it, and see the next section if you're experiencing issues using the .sbsar files inside Unity.

### Preset Files

All .sbsar files have presets built-in, though at the time of writing, the Substance3D for Unity plugin does not support reading these presets. As an alternative, a `presets` folder is included with each .sbsar file that contains the various presets that can be applied to the Susbtances.

### URP and HDRP Support

This asset contains a `Materials_URP.unitypackage`. When using URP, make sure to uncheck imports for the `Materials` and `MaterialsExample` folder, and to include the unitypackage that matches your current rendering pipeline. Once imported, open the unitypackage to import the converted materials into the asset folder.

### Helper Consts File

This package contains a helper C# file under the namespace `PolyLabs.PNTD.Sample`. The classes within the namespace correspond to the Substance names, and each const is a helper string for accessing a property of the Substance. To help you as you code, docstrings are provided detailing the property type, description, and value ranges where applicable.

### Baking Static Images from .sbsar Files

If you're having trouble utilizing Substances in Unity, or don't want to use the Substances in your project, you can still customize the materials and export bitmaps using Adobes free [Substance Player](https://substance3d.adobe.com/documentation/sp31/user-s-guide-for-substance-player-2294742.html).

# Sbsar Materials Reference:


## **BeachSand**
Consts: `PolyLabs.PNTD.Sample.BeachSand`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Color Setting | color_setting | The color of the sand. | Int | Enum </br> Default: 1 <br/> Enum Values: <br/>1: Beach Sand<br/>2: Desert Sand<br/>3: Sahara<br/>4: Red<br/> |
| Sand Waves Rotation | sand_waves_rotation | The rotation of the sand. | Float | Default: 45 <br/> Min: -360 <br/> Max: 360 <br/> Clamped: False |
| Invert Normal Map | invert_normal_map | Inverts the normal map direction for compatability with OpenGL and DirectX. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Sand Roughness | sand_roughness | The roughness of the sand. | Float | Default: 0.8 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |

## **CobbledJungleWall**
Consts: `PolyLabs.PNTD.Sample.CobbledJungleWall`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Rock Type | rock_type | The type of rock color to use with this material. | Int | Enum </br> Default: 4 <br/> Enum Values: <br/>1: Basalt<br/>2: Orange Sandstone<br/>3: Sandstone<br/>4: Shale<br/>5: Siltstone<br/>6: DirtRock<br/>7: Dark Shale<br/>8: Volcanic Rock<br/>9: Limestone<br/> |
| Rocks Scale | rocks_scale | The scale of the rocks. | Int | Default: 6 <br/> Min: 1 <br/> Max: 32 <br/> Clamped: False |
| Rock Highlights Opacity | rock_highlights_opacity | Controls the edge highlight strength on the rocks. | Float | Default: 0.25 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rock Roughness Range | rock_roughness_range | The range of the roughness on the rock portions of the material. | Float | Default: 0.08 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rock Roughness Position | rock_roughness_position | The position of the roughness on the rock. Higher positions yield rocks that are less reflective. | Float | Default: 0.77 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Rock Normal Intensity | rock_normal_intensity | The <b>Intensity</b> parameter modifies the intensity of height map  | Float | Default: 5 <br/> Min: 0 <br/> Max: 15 <br/> Clamped: False |
| Ambient Occlusion Intensity | ambient_occlusion_intensity | Defines the intensity of the AO to simulate shadowed areas on the material. | Float | Default: 0.04 <br/> Min: 0 <br/> Max: 0.2 <br/> Clamped: True |
| Enable Moss | enable_moss | Enables the moss effect. | Int | Boolean 1 | 0 <br/> Default: 1 |
| Moss Color | moss_color | The moss color preset. | Int | Enum </br> Default: 1 <br/> Enum Values: <br/>1: Green<br/>2: Light Green<br/>3: Dark Green<br/>4: Blue<br/>5: Orange<br/> |
| Moss  Position | moss_position | Determines the height of the moss. | Float | Default: 0.7 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Moss Edge Bleeding | moss_edge_bleeding | How much the moss should bleed from the edges to show stain surrounding mossy areas. | Float | Default: 0.32 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Moss Effect from direction | moss_effect_from_direction | The direction that the moss should prefer and group to. Adjust the moss_directional_position value to increase the visibility of this effect. | Float | Default: 0.127556 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: False |
| Moss Directional Position | moss_directional_position | The intensity of the directional moss. Helps to give a more natural look than strictly height-based moss growth where the moss appears to grow in the direction of a light source. | Float | Default: 0.89 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Enable Vines | enable_vines | Whether to show the vines or not. Turning off when otherwise not present has performance benefits. | Int | Boolean 1 | 0 <br/> Default: 0 |
| Vines Color | vines_color | The color preset for the vines. | Int | Enum </br> Default: 1 <br/> Enum Values: <br/>1: Green<br/>2: Jungle Green<br/>3: Dark Green<br/>4: Dry Gray<br/> |
| Vines Scale | vines_scale | The zoom of the vines. | Int | Default: 4 <br/> Min: 2 <br/> Max: 8 <br/> Clamped: True |
| Enable Dirt | enable_dirt_switch | Enables the dirt effect on the material. Turning it off when not being used will slightly increase material generation performance. | Int | Boolean 1 | 0 <br/> Default: 1 |
| Dirt Color | dirt_color | The color preset of the dirt. | Int | Enum </br> Default: 2 <br/> Enum Values: <br/>1: Default<br/>2: Darker<br/>3: Sludgy<br/>4: Dry<br/>5: Graveled<br/> |
| Dirt Mound Amount | dirt_mound_amount | The numerosity of dirt mounds on the material. | Int | Default: 10 <br/> Min: 1 <br/> Max: 32 <br/> Clamped: True |
| Dirt Mound Range | dirt_mound_range | The height range of the dirt mounds. Higher range means higher peaks and lower valleys. | Float | Default: 1 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Dirt Mound Position | dirt_mound_position | The position of the dirt mounds. 0.5 is a median value that maximizes range, while higher will create more flooding towards peaks, and lower will create more flooding towards valleys. | Float | Default: 0.36 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Dirt Mound Contrast | dirt_mound_contrast | The contrast between the dirt mound peaks and valleys. Higher contrast will create larger valleys and more capped peaks. | Float | Default: 0 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Dirt Pebbles Height | dirt_pebbles_height | The height of the dirt pebbles on the material. 0 will hide the pebbles. | Float | Default: 0.5 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Dirt Pebbles Zoom | dirt_pebbles_zoom | Amount of pebbles to show on the x and y axis. | Int | Default: 10 <br/> Min: 1 <br/> Max: 32 <br/> Clamped: False |
| Dirt Pebbles Scale | dirt_pebbles_scale | The size of the individual pebbles. | Float | Default: 1 <br/> Min: 0 <br/> Max: 3 <br/> Clamped: False |
| Dirt Mixing Offset | dirt_mixing_offset | The height offset of the dirt. Use this as the master control to change dirt flooding. | Float | Default: 0.1 <br/> Min: 0 <br/> Max: 1 <br/> Clamped: True |
| Invert Normal Map | invert_normal_map | Inverts the normal map direction for compatability with OpenGL and DirectX. | Int | Boolean 1 | 0 <br/> Default: 0 |

## **FreshSnow**
Consts: `PolyLabs.PNTD.Sample.FreshSnow`
| Label | Identifier | Description | Type | Notes |
|-------|------------|-------------|------|-------|
| Snow Color | snow_color | The main color of the snow | Int | Enum </br> Default: 1 <br/> Enum Values: <br/>1: Default<br/>2: White<br/>3: Blued<br/> |
| Snow Pattern Zoom | snow_pattern_zoom | The zoom of the snow pattern. | Int | Default: 9 <br/> Min: 1 <br/> Max: 16 <br/> Clamped: True |
| Snow Softness | snow_softness | The softness of the snow waves | Float | Default: 8 <br/> Min: 0 <br/> Max: 16 <br/> Clamped: False |
| Invert Normal Map | invert_normal_map | Inverts the normal map direction for compatability with OpenGL and DirectX. | Int | Boolean 1 | 0 <br/> Default: 0 |
