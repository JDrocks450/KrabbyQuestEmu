# Required Blender information.
bl_info = {
           "name": "My Exporter",
           "author": "",
           "version": (1, 0),
           "blender": (2, 65, 0),
           "location": "File > Export > Test (.tst)",
           "description": "",
           "warning": "",
           "wiki_url": "",
           "tracker_url": "",
           "category": "Import-Export"
          }

# Import the Blender required namespaces.
import sys, getopt

import bpy
from bpy_extras.io_utils import ExportHelper



# The main exporter class.
class MyExporter(bpy.types.Operator, ExportHelper):
   bl_idname       = "export_scene.my_exporter";
   bl_label        = "My Exporter";
   bl_options      = {'PRESET'};

   filename_ext    = ".tst";

   object_count    = 0;

   def __init__(self):
      pass

   def execute(self, context):
      print("Execute was called.");

      self.parse_command_line_options();

      if (self.filepath == ""):
         print("No suitable filename was provided to save to.");
         return {'FINISHED'};

      # Get all the mesh objects in the scene.
      objList = [object for object in bpy.context.scene.objects if object.type == 'MESH'];

      # Now process all the objects that we found.
      for gameObject in objList:
         self.export_object(gameObject);

      # Parse all the objects in the scene.
      return {'FINISHED'};


   def export_object(self, gameObject):
      if (gameObject.type != "MESH"):
         print("Object was not of type mesh.");
      else:
         self.object_count += 1;

      return;


   def parse_command_line_options(self):
      modelFile = "";
      myArgs = [];
      argsStartPos = 0;

      if (("--" in sys.argv) == False):
         return;

      argsStartPos = sys.argv.index("--");
      argsStartPos += 1;
      myArgs = sys.argv[argsStartPos:];

      try:
         opts, args = getopt.getopt(myArgs, 'hm:', ["help", "model-file="]);
      except getOpt.GetoptError:
         print("Opt Error.");
         return;

      for opt, arg in opts:
         if (opt in ("-h", "--help")):
            print("Run this as the following blender command.");
            print("\tblender <blend file> --background --python <script file> -- -m <output file>");
         elif (opt in ("-m", "--model-file")):
            modelFile = arg;

      if (modelFile != ""):
         self.filepath = modelFile;



# Define a function to create the menu option for exporting.
def create_menu(self, context):
   self.layout.operator(MyExporter.bl_idname,text="test (.tst)");

# Define the Blender required registration functions.
def register():
   """
   Handles the registration of the Blender Addon.
   """
   bpy.utils.register_module(__name__);
   bpy.types.INFO_MT_file_export.append(create_menu);

def unregister():
   """
   Handles the unregistering of this Blender Addon.
   """
   bpy.utils.unregister_module(__name__);
   bpy.types.INFO_MT_file_export.remove(create_menu);


# Handle running the script from Blender's text editor.
if (__name__ == "__main__"):
   print("Registering.");
   register();

   print("Executing.");
   bpy.ops.export_scene.my_exporter();
