
# want to make vertex at location of (Lowercase Letter)(Number)  \d[a-z]
#delete everything else
# plane for everything on the same C axis
# plane for all the points that differ only on c axis
import bpy
import math
import re
findVertex = re.compile('[A-Z]\d')
for obj in bpy.data.objects:
    if(not findVertex.match(obj.name)):
        bpy.data.objects.remove(obj, do_unlink = True)
    else:
        bpy.ops.object.select_all(action="SELECT")
        bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='MEDIAN')
