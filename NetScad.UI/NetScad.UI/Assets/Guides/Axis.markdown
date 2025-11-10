 
# ![Logo](Assets/Images/logo.png) NetSCAD Custom Axis Guide
###### 
#### Description
Welcome to the NetSCAD Axis Guide! This tutorial covers the basics of defining and generating custom axes used in OpenSCAD, a solid 3D CAD modeler. This project aims to simplify the process of creating complex 3D models by providing reusable components and utilities. Currently, it includes automating the creation of custom Imperial and Metric axes. This allows for more precise modeling used in 3D printing.
###### 
#### Prerequisites
* **You need to have the following installed:** [OpenSCAD](https://openscad.org/downloads.html)
###### 
#### Types of Measurements
Varying axes of different sizes, measurement types, colors, and combinations of both metric and imperial measurements can be applied within the same SCAD project. Axes are managed in an aggregate SCAD file and are called as modules within your SCAD project. The app manages updates to existing axes as well as provides the ability for multiple axis types to be used in your project. Axis increments are as follows:
###### 
* **Metric axis** - 20mm, 10mm, 5mm, and 1mm increments
* **Imperial axis** - 1/4", 1/8", 1/16", and 1/32" increments
* **Convert mm to inches** - Enter inputs in Metric (mm) first, then select Imperial (in)
* **Convert inches to mm** - Enter inputs in Imperial (in) first, then select Metric (mm)
###### 
#### Layout of Custom Axes Builder
The Custom Axes Builder has 2 main visual sections: Custom Axis and Generated Axes. The outputs of newly created axes will show up below the Custom Axis section. This shows the total cubic size, as well as the name to use in your project file. In the list below the custom axis settings, you will see the newly created axis, along with axes that have been previously created. There are two tables that display axes information. One is metric axes and the other is imperial axes. **Both ascending and descending sorting are possible, as well as sorting by multiple categories (shift + select).**
###### 
![AxisCreation](Assets/Images/axisCreation.png)
###### 
#### Setting Inputs for New Axis
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
#### Imperial and Metrix Axes Tables Actions
| Actions                                                                       | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. **Clipboard Icon** - opens a modal showing the calling method to use       | Imperial or Metrix Axes Table           |               |
| 3. **Sorting** - click on one or more column headers to sort (shift + click)  |       Imperial or Metrix Axes Table     |               |

###### 
#### Usage of Custom Axes in OpenSCAD
Axes are stored in a **Scad/Axes** folder that is included with the application. **The axes.scad and individual axis SCAD files within the Axes folder are NOT designed to be edited**, as they are managed by the application itself. Please understand if editing these files, it could break functionality of use of these files within the application itself.

###### 
* **Axes Module Name Format:** Get_ + { Theme } + { X Range } +  { Y Range } +  { Z Range } + { Unit Scale } + Origin + { Min X } + { Min Y } + { Min Z } (N = negative, i.e, N1 is -1)

###### 
| Files                           | Usage In Your SCAD File | Optional parameters |
| ------------------------------- |:-----------------------:|:-------------------:|
| Scad/Axes/axes.scad             | use <Axes/axes.scad>;   |                     |
| Scad/Axes/custom_axis_name.scad | Get_Custom_Axis_Name(); | colorVal, alpha     |