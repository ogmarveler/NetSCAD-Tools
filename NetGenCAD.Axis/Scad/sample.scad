// Axes module:
use <Axes/axes.scad>;

// Uncomment one of the following to display the desired axis style and scale

// Metric 1mm increments
//Get_Light_40x80x120_MM_Orig_0x0xN60();
//Get_Dark_40x80x120_MM_Orig_0x0xN60();

// 1/32 inch increments
//Get_Dark_1x3x4_Inch_Orig_0x0xN2();
Get_Light_1x3x4_Inch_Orig_0x0xN2();

// Basic shapes for scale reference - 1 inch (25.4mm) cube
for (y = [0:25.4:50.8])
	color("purple", 1) translate ([0, y,0]) cube ([25.4, 25.4, 25.4]);

for (z = [0:25.4:76.2])
	color("purple", 1) translate ([0, 0, 25.4 - z]) cube ([25.4, 25.4, 25.4]);