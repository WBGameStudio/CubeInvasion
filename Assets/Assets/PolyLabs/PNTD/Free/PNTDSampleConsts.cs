// Copyright 2022 PolyLabs
// This const file is meant as a helper tool for accessing ids of sbsar files in Unity.
// Docstrings provided give detail to programmers on the acceptable limits of Substance attributes.
namespace PolyLabs.PNTD.Sample
{
	public static class BeachSand
	{
		/// <summary>
		/// Label: Color Setting <br/>
		/// Identifier: color_setting <br/>
		/// Type: Int <br/>
		/// The color of the sand. <br/>
		/// Default: 1<br/>
		/// Enum values:<br/>
		/// 1: Beach Sand<br/>
		/// 2: Desert Sand<br/>
		/// 3: Sahara<br/>
		/// 4: Red<br/>
		/// </summary>
		public const string Color_Setting = "color_setting";
		/// <summary>
		/// Label: Sand Waves Rotation <br/>
		/// Identifier: sand_waves_rotation <br/>
		/// Type: Float <br/>
		/// The rotation of the sand. <br/>
		/// Default: 45 Min: -360 Max: 360 Clamped: False<br/>
		/// </summary>
		public const string Sand_Waves_Rotation = "sand_waves_rotation";
		/// <summary>
		/// Label: Invert Normal Map <br/>
		/// Identifier: invert_normal_map <br/>
		/// Type: Int (boolean 1 | 0)<br/>
		/// Inverts the normal map direction for compatability with OpenGL and DirectX. <br/>
		/// Default: 0<br/>
		/// </summary>
		public const string Invert_Normal_Map = "invert_normal_map";
		/// <summary>
		/// Label: Sand Roughness <br/>
		/// Identifier: sand_roughness <br/>
		/// Type: Float <br/>
		/// The roughness of the sand. <br/>
		/// Default: 0.8 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Sand_Roughness = "sand_roughness";
	}
	public static class CobbledJungleWall
	{
		/// <summary>
		/// Label: Rock Type <br/>
		/// Identifier: rock_type <br/>
		/// Type: Int <br/>
		/// The type of rock color to use with this material. <br/>
		/// Default: 4<br/>
		/// Enum values:<br/>
		/// 1: Basalt<br/>
		/// 2: Orange Sandstone<br/>
		/// 3: Sandstone<br/>
		/// 4: Shale<br/>
		/// 5: Siltstone<br/>
		/// 6: DirtRock<br/>
		/// 7: Dark Shale<br/>
		/// 8: Volcanic Rock<br/>
		/// 9: Limestone<br/>
		/// </summary>
		public const string Rock_Type = "rock_type";
		/// <summary>
		/// Label: Rocks Scale <br/>
		/// Identifier: rocks_scale <br/>
		/// Type: Int <br/>
		/// The scale of the rocks. <br/>
		/// Default: 6 Min: 1 Max: 32 Clamped: False<br/>
		/// </summary>
		public const string Rocks_Scale = "rocks_scale";
		/// <summary>
		/// Label: Rock Highlights Opacity <br/>
		/// Identifier: rock_highlights_opacity <br/>
		/// Type: Float <br/>
		/// Controls the edge highlight strength on the rocks. <br/>
		/// Default: 0.25 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Rock_Highlights_Opacity = "rock_highlights_opacity";
		/// <summary>
		/// Label: Rock Roughness Range <br/>
		/// Identifier: rock_roughness_range <br/>
		/// Type: Float <br/>
		/// The range of the roughness on the rock portions of the material. <br/>
		/// Default: 0.08 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Rock_Roughness_Range = "rock_roughness_range";
		/// <summary>
		/// Label: Rock Roughness Position <br/>
		/// Identifier: rock_roughness_position <br/>
		/// Type: Float <br/>
		/// The position of the roughness on the rock. Higher positions yield rocks that are less reflective. <br/>
		/// Default: 0.77 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Rock_Roughness_Position = "rock_roughness_position";
		/// <summary>
		/// Label: Rock Normal Intensity <br/>
		/// Identifier: rock_normal_intensity <br/>
		/// Type: Float <br/>
		/// The <b>Intensity</b> parameter modifies the intensity of height map  <br/>
		/// Default: 5 Min: 0 Max: 15 Clamped: False<br/>
		/// </summary>
		public const string Rock_Normal_Intensity = "rock_normal_intensity";
		/// <summary>
		/// Label: Ambient Occlusion Intensity <br/>
		/// Identifier: ambient_occlusion_intensity <br/>
		/// Type: Float <br/>
		/// Defines the intensity of the AO to simulate shadowed areas on the material. <br/>
		/// Default: 0.04 Min: 0 Max: 0.2 Clamped: True<br/>
		/// </summary>
		public const string Ambient_Occlusion_Intensity = "ambient_occlusion_intensity";
		/// <summary>
		/// Label: Enable Moss <br/>
		/// Identifier: enable_moss <br/>
		/// Type: Int (boolean 1 | 0)<br/>
		/// Enables the moss effect. <br/>
		/// Default: 1<br/>
		/// </summary>
		public const string Enable_Moss = "enable_moss";
		/// <summary>
		/// Label: Moss Color <br/>
		/// Identifier: moss_color <br/>
		/// Type: Int <br/>
		/// The moss color preset. <br/>
		/// Default: 1<br/>
		/// Enum values:<br/>
		/// 1: Green<br/>
		/// 2: Light Green<br/>
		/// 3: Dark Green<br/>
		/// 4: Blue<br/>
		/// 5: Orange<br/>
		/// </summary>
		public const string Moss_Color = "moss_color";
		/// <summary>
		/// Label: Moss  Position <br/>
		/// Identifier: moss_position <br/>
		/// Type: Float <br/>
		/// Determines the height of the moss. <br/>
		/// Default: 0.7 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Moss__Position = "moss_position";
		/// <summary>
		/// Label: Moss Edge Bleeding <br/>
		/// Identifier: moss_edge_bleeding <br/>
		/// Type: Float <br/>
		/// How much the moss should bleed from the edges to show stain surrounding mossy areas. <br/>
		/// Default: 0.32 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Moss_Edge_Bleeding = "moss_edge_bleeding";
		/// <summary>
		/// Label: Moss Effect from direction <br/>
		/// Identifier: moss_effect_from_direction <br/>
		/// Type: Float <br/>
		/// The direction that the moss should prefer and group to. Adjust the moss_directional_position value to increase the visibility of this effect. <br/>
		/// Default: 0.127556 Min: 0 Max: 1 Clamped: False<br/>
		/// </summary>
		public const string Moss_Effect_from_direction = "moss_effect_from_direction";
		/// <summary>
		/// Label: Moss Directional Position <br/>
		/// Identifier: moss_directional_position <br/>
		/// Type: Float <br/>
		/// The intensity of the directional moss. Helps to give a more natural look than strictly height-based moss growth where the moss appears to grow in the direction of a light source. <br/>
		/// Default: 0.89 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Moss_Directional_Position = "moss_directional_position";
		/// <summary>
		/// Label: Enable Vines <br/>
		/// Identifier: enable_vines <br/>
		/// Type: Int (boolean 1 | 0)<br/>
		/// Whether to show the vines or not. Turning off when otherwise not present has performance benefits. <br/>
		/// Default: 0<br/>
		/// </summary>
		public const string Enable_Vines = "enable_vines";
		/// <summary>
		/// Label: Vines Color <br/>
		/// Identifier: vines_color <br/>
		/// Type: Int <br/>
		/// The color preset for the vines. <br/>
		/// Default: 1<br/>
		/// Enum values:<br/>
		/// 1: Green<br/>
		/// 2: Jungle Green<br/>
		/// 3: Dark Green<br/>
		/// 4: Dry Gray<br/>
		/// </summary>
		public const string Vines_Color = "vines_color";
		/// <summary>
		/// Label: Vines Scale <br/>
		/// Identifier: vines_scale <br/>
		/// Type: Int <br/>
		/// The zoom of the vines. <br/>
		/// Default: 4 Min: 2 Max: 8 Clamped: True<br/>
		/// </summary>
		public const string Vines_Scale = "vines_scale";
		/// <summary>
		/// Label: Enable Dirt <br/>
		/// Identifier: enable_dirt_switch <br/>
		/// Type: Int (boolean 1 | 0)<br/>
		/// Enables the dirt effect on the material. Turning it off when not being used will slightly increase material generation performance. <br/>
		/// Default: 1<br/>
		/// </summary>
		public const string Enable_Dirt = "enable_dirt_switch";
		/// <summary>
		/// Label: Dirt Color <br/>
		/// Identifier: dirt_color <br/>
		/// Type: Int <br/>
		/// The color preset of the dirt. <br/>
		/// Default: 2<br/>
		/// Enum values:<br/>
		/// 1: Default<br/>
		/// 2: Darker<br/>
		/// 3: Sludgy<br/>
		/// 4: Dry<br/>
		/// 5: Graveled<br/>
		/// </summary>
		public const string Dirt_Color = "dirt_color";
		/// <summary>
		/// Label: Dirt Mound Amount <br/>
		/// Identifier: dirt_mound_amount <br/>
		/// Type: Int <br/>
		/// The numerosity of dirt mounds on the material. <br/>
		/// Default: 10 Min: 1 Max: 32 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mound_Amount = "dirt_mound_amount";
		/// <summary>
		/// Label: Dirt Mound Range <br/>
		/// Identifier: dirt_mound_range <br/>
		/// Type: Float <br/>
		/// The height range of the dirt mounds. Higher range means higher peaks and lower valleys. <br/>
		/// Default: 1 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mound_Range = "dirt_mound_range";
		/// <summary>
		/// Label: Dirt Mound Position <br/>
		/// Identifier: dirt_mound_position <br/>
		/// Type: Float <br/>
		/// The position of the dirt mounds. 0.5 is a median value that maximizes range, while higher will create more flooding towards peaks, and lower will create more flooding towards valleys. <br/>
		/// Default: 0.36 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mound_Position = "dirt_mound_position";
		/// <summary>
		/// Label: Dirt Mound Contrast <br/>
		/// Identifier: dirt_mound_contrast <br/>
		/// Type: Float <br/>
		/// The contrast between the dirt mound peaks and valleys. Higher contrast will create larger valleys and more capped peaks. <br/>
		/// Default: 0 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mound_Contrast = "dirt_mound_contrast";
		/// <summary>
		/// Label: Dirt Pebbles Height <br/>
		/// Identifier: dirt_pebbles_height <br/>
		/// Type: Float <br/>
		/// The height of the dirt pebbles on the material. 0 will hide the pebbles. <br/>
		/// Default: 0.5 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Pebbles_Height = "dirt_pebbles_height";
		/// <summary>
		/// Label: Dirt Pebbles Zoom <br/>
		/// Identifier: dirt_pebbles_zoom <br/>
		/// Type: Int <br/>
		/// Amount of pebbles to show on the x and y axis. <br/>
		/// Default: 10 Min: 1 Max: 32 Clamped: False<br/>
		/// </summary>
		public const string Dirt_Pebbles_Zoom = "dirt_pebbles_zoom";
		/// <summary>
		/// Label: Dirt Pebbles Scale <br/>
		/// Identifier: dirt_pebbles_scale <br/>
		/// Type: Float <br/>
		/// The size of the individual pebbles. <br/>
		/// Default: 1 Min: 0 Max: 3 Clamped: False<br/>
		/// </summary>
		public const string Dirt_Pebbles_Scale = "dirt_pebbles_scale";
		/// <summary>
		/// Label: Dirt Mixing Offset <br/>
		/// Identifier: dirt_mixing_offset <br/>
		/// Type: Float <br/>
		/// The height offset of the dirt. Use this as the master control to change dirt flooding. <br/>
		/// Default: 0.1 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mixing_Offset = "dirt_mixing_offset";
		/// <summary>
		/// Label: Invert Normal Map <br/>
		/// Identifier: invert_normal_map <br/>
		/// Type: Int (boolean 1 | 0)<br/>
		/// Inverts the normal map direction for compatability with OpenGL and DirectX. <br/>
		/// Default: 0<br/>
		/// </summary>
		public const string Invert_Normal_Map = "invert_normal_map";
	}
	public static class FreshSnow
	{
		/// <summary>
		/// Label: Snow Color <br/>
		/// Identifier: snow_color <br/>
		/// Type: Int <br/>
		/// The main color of the snow <br/>
		/// Default: 1<br/>
		/// Enum values:<br/>
		/// 1: Default<br/>
		/// 2: White<br/>
		/// 3: Blued<br/>
		/// </summary>
		public const string Snow_Color = "snow_color";
		/// <summary>
		/// Label: Snow Pattern Zoom <br/>
		/// Identifier: snow_pattern_zoom <br/>
		/// Type: Int <br/>
		/// The zoom of the snow pattern. <br/>
		/// Default: 9 Min: 1 Max: 16 Clamped: True<br/>
		/// </summary>
		public const string Snow_Pattern_Zoom = "snow_pattern_zoom";
		/// <summary>
		/// Label: Snow Softness <br/>
		/// Identifier: snow_softness <br/>
		/// Type: Float <br/>
		/// The softness of the snow waves <br/>
		/// Default: 8 Min: 0 Max: 16 Clamped: False<br/>
		/// </summary>
		public const string Snow_Softness = "snow_softness";
		/// <summary>
		/// Label: Invert Normal Map <br/>
		/// Identifier: invert_normal_map <br/>
		/// Type: Int (boolean 1 | 0)<br/>
		/// Inverts the normal map direction for compatability with OpenGL and DirectX. <br/>
		/// Default: 0<br/>
		/// </summary>
		public const string Invert_Normal_Map = "invert_normal_map";
	}
	public static class cavernous_rock
	{
		/// <summary>
		/// Label: Rock Type <br/>
		/// Identifier: rock_type <br/>
		/// Type: Int <br/>
		/// The type of rock color to use with this material. <br/>
		/// Default: 4<br/>
		/// Enum values:<br/>
		/// 1: Basalt<br/>
		/// 2: Orange Sandstone<br/>
		/// 3: Sandstone<br/>
		/// 4: Shale<br/>
		/// 5: Siltstone<br/>
		/// 6: DirtRock<br/>
		/// 7: Dark Shale<br/>
		/// 8: Volcanic Rock<br/>
		/// 9: Limestone<br/>
		/// </summary>
		public const string Rock_Type = "rock_type";
		/// <summary>
		/// Label: Rock Highlights Opacity <br/>
		/// Identifier: rock_highlights_opacity <br/>
		/// Type: Float <br/>
		/// Controls the edge highlight strength on the rocks. <br/>
		/// Default: 0.25 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Rock_Highlights_Opacity = "rock_highlights_opacity";
		/// <summary>
		/// Label: Rock Roughness Range <br/>
		/// Identifier: rock_roughness_range <br/>
		/// Type: Float <br/>
		/// The range of the roughness on the rock portions of the material. <br/>
		/// Default: 0.08 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Rock_Roughness_Range = "rock_roughness_range";
		/// <summary>
		/// Label: Rock Roughness Position <br/>
		/// Identifier: rock_roughness_position <br/>
		/// Type: Float <br/>
		/// The position of the roughness on the rock. Higher positions yield rocks that are less reflective. <br/>
		/// Default: 0.77 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Rock_Roughness_Position = "rock_roughness_position";
		/// <summary>
		/// Label: Rock Normal Intensity <br/>
		/// Identifier: rock_normal_intensity <br/>
		/// Type: Float <br/>
		/// The <b>Intensity</b> parameter modifies the intensity of height map  <br/>
		/// Default: 5 Min: 0 Max: 15 Clamped: False<br/>
		/// </summary>
		public const string Rock_Normal_Intensity = "rock_normal_intensity";
		/// <summary>
		/// Label: Ambient Occlusion Intensity <br/>
		/// Identifier: ambient_occlusion_intensity <br/>
		/// Type: Float <br/>
		/// Defines the intensity of the AO to simulate shadowed areas on the material. <br/>
		/// Default: 0.2 Min: 0 Max: 0.2 Clamped: True<br/>
		/// </summary>
		public const string Ambient_Occlusion_Intensity = "ambient_occlusion_intensity";
		/// <summary>
		/// Label: Rock Pattern X Amount <br/>
		/// Identifier: rock_pattern_x_amount <br/>
		/// Type: Int <br/>
		/// The amount of patterns on the x-axis <br/>
		/// Default: 6 Min: 1 Max: 12 Clamped: True<br/>
		/// </summary>
		public const string Rock_Pattern_X_Amount = "rock_pattern_x_amount";
		/// <summary>
		/// Label: Rock Pattern Y Amount <br/>
		/// Identifier: rock_pattern_y_amount <br/>
		/// Type: Int <br/>
		/// The amount of patterns on the y-axis. <br/>
		/// Default: 6 Min: 1 Max: 12 Clamped: True<br/>
		/// </summary>
		public const string Rock_Pattern_Y_Amount = "rock_pattern_y_amount";
		/// <summary>
		/// Label: Enable Dirt <br/>
		/// Identifier: enable_dirt_switch <br/>
		/// Type: Int (boolean 1 | 0)<br/>
		/// Enables the dirt effect on the material. Turning it off when not being used will slightly increase material generation performance. <br/>
		/// Default: 0<br/>
		/// </summary>
		public const string Enable_Dirt = "enable_dirt_switch";
		/// <summary>
		/// Label: Dirt Mound Amount <br/>
		/// Identifier: dirt_mound_amount <br/>
		/// Type: Int <br/>
		/// The numerosity of dirt mounds on the material. <br/>
		/// Default: 10 Min: 1 Max: 32 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mound_Amount = "dirt_mound_amount";
		/// <summary>
		/// Label: Dirt Mound Range <br/>
		/// Identifier: dirt_mound_range <br/>
		/// Type: Float <br/>
		/// The height range of the dirt mounds. Higher range means higher peaks and lower valleys. <br/>
		/// Default: 1 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mound_Range = "dirt_mound_range";
		/// <summary>
		/// Label: Dirt Mound Position <br/>
		/// Identifier: dirt_mound_position <br/>
		/// Type: Float <br/>
		/// The position of the dirt mounds. 0.5 is a median value that maximizes range, while higher will create more flooding towards peaks, and lower will create more flooding towards valleys. <br/>
		/// Default: 0.5 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mound_Position = "dirt_mound_position";
		/// <summary>
		/// Label: Dirt Mound Contrast <br/>
		/// Identifier: dirt_mound_contrast <br/>
		/// Type: Float <br/>
		/// The contrast between the dirt mound peaks and valleys. Higher contrast will create larger valleys and more capped peaks. <br/>
		/// Default: 0 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mound_Contrast = "dirt_mound_contrast";
		/// <summary>
		/// Label: Dirt Normal Intensity <br/>
		/// Identifier: dirt_normal_intensity <br/>
		/// Type: Float <br/>
		/// The <b>Intensity</b> parameter modifies the intensity of the normal map  <br/>
		/// Default: 5 Min: 0 Max: 10 Clamped: False<br/>
		/// </summary>
		public const string Dirt_Normal_Intensity = "dirt_normal_intensity";
		/// <summary>
		/// Label: Dirt Smoothness <br/>
		/// Identifier: dirt_smoothness <br/>
		/// Type: Float <br/>
		/// How smooth the dirt appears. Lower values will give a more grainy, earthy feel. <br/>
		/// Default: 0.8 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Smoothness = "dirt_smoothness";
		/// <summary>
		/// Label: Dirt Roughness Range <br/>
		/// Identifier: dirt_roughness_range <br/>
		/// Type: Float <br/>
		/// The range of roughness levels present on the material. High range means there will be areas with very high and low roughness. <br/>
		/// Default: 0.1 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Roughness_Range = "dirt_roughness_range";
		/// <summary>
		/// Label: Dirt Roughness Position <br/>
		/// Identifier: dirt_roughness_position <br/>
		/// Type: Float <br/>
		/// The position of the dirt roughness. Higher values equates to a rougher look and less reflectivity. <br/>
		/// Default: 0.85 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Roughness_Position = "dirt_roughness_position";
		/// <summary>
		/// Label: Dirt Mixing Offset <br/>
		/// Identifier: dirt_mixing_offset <br/>
		/// Type: Float <br/>
		/// The height offset of the dirt. Use this as the master control to change dirt flooding. <br/>
		/// Default: 0.29 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mixing_Offset = "dirt_mixing_offset";
		/// <summary>
		/// Label: Dirt Mixing Contrast <br/>
		/// Identifier: dirt_mixing_contrast <br/>
		/// Type: Float <br/>
		/// The contrast between the material and dirt. Higher contrast leads to a more crisp cutoff between dirt and the main material. <br/>
		/// Default: 0.9 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Dirt_Mixing_Contrast = "dirt_mixing_contrast";
		/// <summary>
		/// Label: Enable Water <br/>
		/// Identifier: enable_water_switch <br/>
		/// Type: Int (boolean 1 | 0)<br/>
		/// Enable or disable the water feature on the material. Disabling when not used has performance benefits. <br/>
		/// Default: 0<br/>
		/// </summary>
		public const string Enable_Water = "enable_water_switch";
		/// <summary>
		/// Label: Water Level <br/>
		/// Identifier: water_level <br/>
		/// Type: Float <br/>
		/// The amount of flooding on the material. <br/>
		/// Default: 0.25 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Water_Level = "water_level";
		/// <summary>
		/// Label: Water Darkness <br/>
		/// Identifier: water_darkness <br/>
		/// Type: Float <br/>
		/// The amount to darken the areas that are submerged on the material <br/>
		/// Default: 0.1 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Water_Darkness = "water_darkness";
		/// <summary>
		/// Label: Water Edges Wetness <br/>
		/// Identifier: water_edges_wetness <br/>
		/// Type: Float <br/>
		/// How we the edges where the material meets the water appears. <br/>
		/// Default: 0.1 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Water_Edges_Wetness = "water_edges_wetness";
		/// <summary>
		/// Label: Water Edges Wetness Distance <br/>
		/// Identifier: water_edges_wetness_distance <br/>
		/// Type: Float <br/>
		/// How far the edge wetness spreads on the material. <br/>
		/// Default: 0.1 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Water_Edges_Wetness_Distance = "water_edges_wetness_distance";
		/// <summary>
		/// Label: Rock Divots Intensity <br/>
		/// Identifier: rock_divots_intensity <br/>
		/// Type: Float <br/>
		/// The intensity of the rock divots. <br/>
		/// Default: 0.04 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Rock_Divots_Intensity = "rock_divots_intensity";
		/// <summary>
		/// Label: Rock Edges Opacity <br/>
		/// Identifier: rock_edges_opacity <br/>
		/// Type: Float <br/>
		/// The opacity of the rocky edges. <br/>
		/// Default: 0.28 Min: 0 Max: 1 Clamped: True<br/>
		/// </summary>
		public const string Rock_Edges_Opacity = "rock_edges_opacity";
		/// <summary>
		/// Label: Invert Normal Map <br/>
		/// Identifier: invert_normal_map <br/>
		/// Type: Int (boolean 1 | 0)<br/>
		/// Inverts the normal map direction for compatability with OpenGL and DirectX. <br/>
		/// Default: 0<br/>
		/// </summary>
		public const string Invert_Normal_Map = "invert_normal_map";
	}
}
