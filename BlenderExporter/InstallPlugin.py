import bpy

bpy.ops.preferences.addon_install(overwrite=True, filepath="C://Users//xXJDr//source//repos//KrabbyQuestTools//Krabby Quest Installer//bin//Debug//plugin.zip")
bpy.ops.preferences.addon_enable(module="io_scene_b3d")
bpy.ops.wm.save_userpref()
