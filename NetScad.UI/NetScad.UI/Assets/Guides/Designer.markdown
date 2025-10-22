 
# ![Logo](Assets/Images/logo.png) NetSCAD Designer Guide
###### 
#### Description
Welcome to the NetSCAD Designer Guide! This tutorial covers the basics of defining and generating custom objects used in OpenSCAD, a solid 3D CAD modeler. This project aims to simplify the process of creating complex 3D models by providing reusable components and utilities. Currently, it includes automating the creation of custom Imperial and Metric axes, as well as quick generation of objects to help speed up the modeling process. This allows for more precise modeling used in 3D printing.
###### 
#### Prerequisites
* **You need to have the following installed:** [OpenSCAD](https://openscad.org/downloads.html)
###### 
#### Types of Solids Available
Cubes and cylinders can be generated from the Designer module within this application. Rounded versions of cubes are available in this iteration, in order to show how to offset Minkowski rounding to align with 0,0 axes. Future iterations will have more solid types available, but if needed, generated outputs can be modified within SCAD files directly. Solids available are as follows:
###### 
* **Cube** - aligned on the 0,0 axes
* **Rounded Cube** - using Minkowski rounding with offsets to align with 0,0 axes
* **Cylinder** - all cylinders are aligned on the 0,0 axes, with the center point of the top of the cylinder (center circle)
* **Rounded Box with Chamfered Edges** - using Minkowski rounding with chamfered edges, aligned on the 0,0 axes, and utilizing the difference function to remove part of the cube cutout
* **Cylinder with Screw Hole Inserts** - Raised cylinder with screw hole inserts for M2 - M10 screws, aligned on the 0,0 axes, with the center point of the top of the cylinder (center circle)
###### 
#### Layout of Custom Object Designer
The Custom Object Designer is organized into several sections to facilitate the creation of custom objects for OpenSCAD. The main sections include:
###### 
![AxesUsageGuide](Assets/Images/designerUsageGuide.png)
###### 
#### Setting Inputs for New Object
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. Open the **Axes** menu and select **Create New Axes**                      |                                         |               |
| 2. Select the **Unit Type**                                                   | Metric (mm) or Imperial (in)            | Metric (mm)   |
| 3. Select the **OpenSCAD** theme used in OpenSCAD                             | Light Theme or Dark Theme               | Light Theme   |
| 4. Enter numeric **Min X** value                                              | Min X **<=** 0 and Min X **<** Max X    | 0 (mm or in)  |
| 5. Enter numeric **Max X** value                                              | Max X **>=** 0 and Max X **>** Min X    | 300mm or 12"  |
| 6. Enter numeric **Min Y** value                                              | Min Y **<=** 0 and Min Y **<** Max Y    | 0 (mm or in)  |
| 7. Enter numeric **Max Y** value                                              | Max Y **>=** 0 and Max Y **>** Min Y    | 300mm or 12"  |
| 8. Enter numeric **Min Z** value                                              | Min Z **<=** 0 and Min Z **<** Max Z    | 0 (mm or in)  |
| 9. Enter numeric **Max Z** value                                              | Max Z **>=** 0 and Max Z **>** Min Z    | 300mm or 12"  |
| 10. Click the **Create Axis** button to generate the defined axis             |                                         |               |
| 11. To revert to default settings, click the **Clear** button                 |                                         |               |
| 12. Call the new axis using the **Calling Method**                            | Optional params: (colorVal, alpha)      | Theme-based   |
| 13. To view output files, open the **Scad/Axes folder** in the app's directory|                                         |               |
###### 
#### Usage of Custom Axes in OpenSCAD
Axes are stored in a **Scad/Axes** folder that is included with the application. A **sample.scad** file is also included to provide a few examples of how to use the custom axes within your project. **The axes.scad and individual axis SCAD files within the Axes folder are NOT designed to be edited**, as they are managed by the application itself. Please understand if editing these files, it could break functionality of use of these files within the application itself.
###### 
* **Axes Module Name Format:** Get_ + { Theme } + { X Range } +  { Y Range } +  { Z Range } + { Unit Scale } + Origin + { Min X } + { Min Y } + { Min Z } (N = negative, i.e, N1 is -1)
###### 
| Files                           | Usage In Your SCAD File | Optional parameters |
| ------------------------------- |:-----------------------:|:-------------------:|
| Scad/Axes/axes.scad             | use <Axes/axes.scad>;   |                     |
| Scad/Axes/custom_axis_name.scad | Get_Custom_Axis_Name(); | colorVal, alpha     |
| Scad/sample.scad                |                         |                     |