/******************************************************************************/
// Axes for reference
// Usage: use <Axes/axes.scad>;  // add to your main file;
// Options: unit="mm" or "in" (default is "mm")
// For Metric, Axis will set measurements to 20mm, 10mm, 5mm, 1mm increments.
// For Imperial, Axis will be set to 1/4, 1/8, 1/16, and 1/32 inch increments.
// For larger measurements, adjust Min, Max, and Scale accordingly to keep axis readable.
/******************************************************************************/

// 3D Axis Module - Light 1x3x4 Inch Orig 0x0xN2
// Calling Method: Get_Get_Light_1x3x4_Inch_Orig_0x0xN2();
// Settings: UnitSystem=Imperial, BackgroundType=Light, MinX=0, MaxX=25.4, MinY=0, MaxY=76.2, MinZ=-50.8, MaxZ=50.8
include <light_1x3x4_inch_orig_0x0xn2.scad>;
module Get_Light_1x3x4_Inch_Orig_0x0xN2(colorVal = "Black", alpha = 1) {
  light_1x3x4_inch_orig_0x0xn2(colorVal = colorVal, alpha = alpha);
}

// 3D Axis Module - Dark 1x3x4 Inch Orig 0x0xN2
// Calling Method: Get_Get_Dark_1x3x4_Inch_Orig_0x0xN2();
// Settings: UnitSystem=Imperial, BackgroundType=Dark, MinX=0, MaxX=25.4, MinY=0, MaxY=76.2, MinZ=-50.8, MaxZ=50.8
include <dark_1x3x4_inch_orig_0x0xn2.scad>;
module Get_Dark_1x3x4_Inch_Orig_0x0xN2(colorVal = "White", alpha = 1) {
  dark_1x3x4_inch_orig_0x0xn2(colorVal = colorVal, alpha = alpha);
}

// 3D Axis Module - Light 40x80x120 MM Orig 0x0xN60
// Calling Method: Get_Get_Light_40x80x120_MM_Orig_0x0xN60();
// Settings: UnitSystem=Metric, BackgroundType=Light, MinX=0, MaxX=40, MinY=0, MaxY=80, MinZ=-60, MaxZ=60
include <light_40x80x120_mm_orig_0x0xn60.scad>;
module Get_Light_40x80x120_MM_Orig_0x0xN60(colorVal = "Black", alpha = 1) {
  light_40x80x120_mm_orig_0x0xn60(colorVal = colorVal, alpha = alpha);
}

// 3D Axis Module - Dark 40x80x120 MM Orig 0x0xN60
// Calling Method: Get_Get_Dark_40x80x120_MM_Orig_0x0xN60();
// Settings: UnitSystem=Metric, BackgroundType=Dark, MinX=0, MaxX=40, MinY=0, MaxY=80, MinZ=-60, MaxZ=60
include <dark_40x80x120_mm_orig_0x0xn60.scad>;
module Get_Dark_40x80x120_MM_Orig_0x0xN60(colorVal = "White", alpha = 1) {
  dark_40x80x120_mm_orig_0x0xn60(colorVal = colorVal, alpha = alpha);
}

