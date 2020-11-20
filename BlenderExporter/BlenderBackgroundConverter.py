import bpy;

#clear scene
context = bpy.context
scene = context.scene

for c in scene.collection.children:
    scene.collection.children.unlink(c)

for c in bpy.data.collections:
    if not c.users:
        bpy.data.collections.remove(c)

#import B3D
bpy.ops.import_scene.blitz3d_b3d(filepath="D:/Projects/Krabby Quest/Workspace/Graphics/rock.b3d", filter_glob="*.b3d", constrain_size=10, 
use_image_search=True, use_apply_transform=True, axis_forward='Z', axis_up='Y')

#export to OBJ
bpy.ops.export_scene.obj(filepath="hewwo", check_existing=True, filter_glob="*.obj;*.mtl", use_selection=False, 
use_animation=False, use_mesh_modifiers=True, use_edges=True, use_smooth_groups=False, 
use_smooth_groups_bitflags=False, use_normals=True, use_uvs=True, use_materials=True, 
use_triangles=False, use_nurbs=False, use_vertex_groups=False, use_blen_objects=True,
group_by_object=False, group_by_material=False, keep_vertex_order=False, 
global_scale=1, path_mode='AUTO', axis_forward='-Z', axis_up='Y')

#export FBX
#bpy.ops.export_scene.fbx(filepath="D:/Projects/Krabby Quest/Workspace/Export/rock.fbx", check_existing=True, filter_glob="*.fbx", 
#ui_tab='MAIN', use_selection=False, 
#use_active_collection=False, global_scale=1, apply_unit_scale=True, apply_scale_options='FBX_SCALE_NONE', 
#bake_space_transform=False, object_types={'EMPTY', 'CAMERA', 'LIGHT', 'ARMATURE', 'MESH', 'OTHER'}, 
#use_mesh_modifiers=True, use_mesh_modifiers_render=True, mesh_smooth_type='OFF', 
#use_mesh_edges=False, use_tspace=False, use_custom_props=False, add_leaf_bones=True, 
#primary_bone_axis='Y', secondary_bone_axis='X', use_armature_deform_only=False, 
#armature_nodetype='NULL', bake_anim=True, bake_anim_use_all_bones=True, 
#bake_anim_use_nla_strips=True, bake_anim_use_all_actions=True, 
#bake_anim_force_startend_keying=True, bake_anim_step=1, 
#bake_anim_simplify_factor=1, path_mode='AUTO', embed_textures=True, batch_mode='OFF', 
#use_batch_own_dir=True, use_metadata=True, axis_forward='-Z', axis_up='Y')