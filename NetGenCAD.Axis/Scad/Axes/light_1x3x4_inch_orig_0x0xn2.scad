// Light_1x3x4_Inch_Orig_0x0xN2 Imperial NetGenCAD.Core Axis Module
// Creates a 3D axis with labeled measurements along the X, Y, and Z axes.
// Parameters:
// - UnitSystem: 'Metric' for millimeters or 'Imperial' for inches (default: Metric)
// - IncrementX, IncrementY, IncrementZ: Spacing between labels on each axis (default: 1.5875mm)
// - MinX, MaxX: Minimum and maximum values for the X axis (default: 0 to 300mm)
// - MinY, MaxY: Minimum and maximum values for the Y axis (default: 0 to 300mm)
// - MinZ, MaxZ: Minimum and maximum values for the Z axis (default: 0 to 300mm)

module light_1x3x4_inch_orig_0x0xn2(colorVal, alpha) {
    color(colorVal, alpha) {
         for (x = [0:6.35:25.4]){   if(x != 0)
 translate([x - .1, -8.75, .1]) cube([0.2, 8.75, 0.02]);   }
         for (y = [0:6.35:76.2]){   if(y != 0)
 translate([-8.75, y - .1, .1]) cube([8.75, 0.2, 0.02]);   }
         for (z = [-50.8:6.35:50.8]){   if(z != 0)
 translate([-7.5, -7.5, z + .1]) rotate([90, 45, 135]) cube([0.2, 0.02, 7.5]);   }
         for (x = [0:3.175:25.4]){   if(x != 0)
 translate([x - .1, -5, .1]) cube([0.2, 5, 0.02]);   }
         for (y = [0:3.175:76.2]){   if(y != 0)
 translate([-5, y - .1, .1]) cube([5, 0.2, 0.02]);   }
         for (z = [-50.8:3.175:50.8]){   if(z != 0)
 translate([-3.75, -3.75, z + .1]) rotate([90, 45, 135]) cube([0.2, 0.02, 5]);   }
         for (x = [0:1.5875:25.4]){   if(x != 0)
 translate([x - .1, -2.5, .1]) cube([0.2, 2.5, 0.02]);   }
         for (y = [0:1.5875:76.2]){   if(y != 0)
 translate([-2.5, y - .1, .1]) cube([2.5, 0.2, 0.02]);   }
         for (z = [-50.8:1.5875:50.8]){   if(z != 0)
 translate([-1.75, -1.75, z + .1]) rotate([90, 45, 135]) cube([0.2, 0.02, 2.5]);   }
         for (x = [0:0.79375:25.4]){   if(x != 0)
 translate([x - .1, -1.25, .1]) cube([0.2, 1.25, 0.02]);   }
         for (y = [0:0.79375:76.2]){   if(y != 0)
 translate([-1.25, y - .1, .1]) cube([1.25, 0.2, 0.02]);   }
         for (z = [-50.8:0.79375:50.8]){   if(z != 0)
 translate([-.875, -.875, z + .1]) rotate([90, 45, 135]) cube([0.2, 0.02, 1.25]);   }
         // Axis Labels
         unit = "in";
         scale = 25.4;

         for (i = [0:6.35:25.4]){   if((i >= scale * .75 - .875 || i <= -scale * .75 + .875) && i != 0)
 translate([i - 0.875, -10, .1]) linear_extrude(0.02) rotate(270) text(str(i/scale, unit), size=2);   }
         for (i = [0:6.35:76.2]){   if((i >= scale * .75 - .875 || i <= -scale * .75 + .875) && i != 0)
 translate([-10, i + 0.875, .1]) linear_extrude(0.02) rotate(180) text(str(i/scale, unit), size=2);   }
         for (i = [-50.8:6.35:50.8]){   if((i >= scale * .75 - .875 || i <= -scale * .75 + .875) && i != 0)
 translate([-8.75, -8.75, i - .875]) rotate([0,45,135]) linear_extrude(0.02) rotate(90) text(str(i/scale, unit), size=1.75);   }
  }
}
// End of Light_1x3x4_Inch_Orig_0x0xN2 Module
