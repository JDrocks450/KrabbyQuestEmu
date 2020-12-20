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
use_image_search=False, use_apply_transform=True, axis_forward='Z', axis_up='Y')

#export to OBJ
bpy.ops.export_scene.gltf(export_format='GLB', export_texcoords=True,
                         export_normals=True, export_draco_mesh_compression_enable=False,
                        export_draco_position_quantization=14, export_draco_normal_quantization=10, export_draco_texcoord_quantization=12,
                       export_tangents=False, export_materials=True, export_colors=True, export_cameras=False, export_selected=False,
                      export_extras=False, export_yup=True, export_apply=False, export_animations=True, export_frame_range=True, export_frame_step=1,
                     export_force_sampling=False, export_current_frame=False, export_skins=True, export_all_influences=False, export_morph=True,
                    export_morph_normal=True, export_morph_tangent=False, export_lights=False, export_displacement=False, will_save_settings=False,
                   filepath="", check_existing=False, filter_glob="*.glb;*.gltf")