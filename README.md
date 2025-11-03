 
# <img src="https://github.com/ogmarveler/NetSCAD/blob/25f80b909ac1837b3adb92b88e1d9d3ce63d0e2c/NetScad.UI/NetScad.UI/Assets/Images/logo-netscad.png" alt="NetSCAD logo" height="30" width="30"> NetSCAD Tools
###### 
#### Description
This application is a low-code / no-code tool for generating custom objects used in OpenSCAD, a solid 3D CAD modeler. This simplifies the process of creating complex 3D models by providing reusable components and utilities. Currently, it includes creation of custom Imperial and Metric axes, as well as quick generation of objects to help speed up the modeling process. This allows for more precise modeling used in 3D printing. **It is assumed that users have a basic understanding of OpenSCAD and its functionalities.**
###### 
#### Prerequisites
* **You need to have the following installed:** [OpenSCAD](https://openscad.org/downloads.html)
######
**Get the latest release of NetSCAD Tools**
* [NetSCAD for Windows (x64)](NetScad.UI/NetScad.UI.Windows/NetSCAD-v0.2.1-win-x64.zip)
* [NetSCAD for Linux (x64)](NetScad.UI/NetScad.UI.Linux/NetSCAD-v0.2.1-linux-x64.tar.gz)
* [NetSCAD for Raspberry Pi (arm64)](NetScad.UI/NetScad.UI.Linux/NetSCAD-v0.2.1-linux-arm64.tar.gz)

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
![Axis Creation](NetScad.UI/NetScad.UI/Assets/Images/axisCreation.png)
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
#### Usage of Custom Axes in OpenSCAD
Axes are stored in a **Scad/Axes** folder that is included with the application. **The axes.scad and individual axis SCAD files within the Axes folder are NOT designed to be edited**, as they are managed by the application itself. Please understand if editing these files, it could break functionality of use of these files within the application itself.
###### 
* **Axes Module Name Format:** Get_ + { Theme } + { X Range } +  { Y Range } +  { Z Range } + { Unit Scale } + Origin + { Min X } + { Min Y } + { Min Z } (N = negative, i.e, N1 is -1)
###### 
| Files                           | Usage In Your SCAD File | Optional parameters |
| ------------------------------- |:-----------------------:|:-------------------:|
| Scad/Axes/axes.scad             | use <Axes/axes.scad>;   |                     |
| Scad/Axes/custom_axis_name.scad | Get_Custom_Axis_Name(); | colorVal, alpha     |
###### 

#### Layout of Custom Object Designer
The Custom Object Designer is organized into several sections to facilitate the creation of custom objects for OpenSCAD. The main sections include:
###### 
![Designer Object](NetScad.UI/NetScad.UI/Assets/Images/designerObject.png)
######
![Object](NetScad.UI/NetScad.UI/Assets/Images/object.png)
######
#### Types of Solids Available
Cubes and cylinders can be generated from the Designer module within this application. Rounded versions of cubes are also available in this iteration, in order to show how to offset Minkowski rounding to align with 0,0 axes. Future iterations will have more solid types available, but if needed, generated outputs can be modified within SCAD files directly. Solids available are as follows:
###### 
* **Cube** - default aligned on the 0,0,0 axes or can be offset with translate
* **Rounded Cube** - using Minkowski rounding with offsets to align with 0,0,0 axes
* **Cylinder** - all cylinders are aligned on the 0,0,0 axes, with the center point of the top of the cylinder (center circle)
###### 
#### Apply a Custom Axis
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. Open the **Create** menu and select **Create New Object**                  |                                         |               |
| 2. Select an **Axis Type** to filter saved Imperial or Metric axes            |       Metric (mm) or Imperial (in)      |  Metric (mm)  |
| 3. Choose from **Select Axis** to apply one of the saved axes                 |     Dark or Light X x Y x Z mm or in    |  <required>   |
| 4. Optional: Enter numeric **Adjust X**, setting start point on **X Axis**    |     Units displayed in mm or inches     |       0       |
| 5. Optional: Enter numeric **Adjust Y**, setting start point on **Y Axis**    |     Units displayed in mm or inches     |       0       |
| 6. Optional: Enter numeric **Adjust Z**, setting start point on **Z Axis**    |     Units displayed in mm or inches     |       0       |

###### 
#### Creating A New Object
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 7. Enter an **Object Name**, used as the main identifier for solids, etc.     |           <Set Object Name>             |  <required>   |
| 8. Select a **Unit Type** to enter dimensions by Imperial or Metric units     |       Metric (mm) or Imperial (in)      |  Metric (mm)  |
| 9. Select a **Solid Type** to be added or subtracted from the object          |    Cube or Rounded Cube or Cylinder     |  <required>   |
| 10. Enter a name for the solid in the **Solid Description** textbox           |        <Set Object Description>         |  <required>   |
| 11. Select an **Apply To** type, which determines solid's relation to object  |    Union or Difference or Intersection  |     Union     |

###### 
#### Setting Inputs for New Solid: Cube and Rounded Cube
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 12. Optional: Select from presets to apply width, height, or cylinder radius  |     Rack Width or Rack Height           |               |
| 13. Enter numeric **Length** (based on 0 origin), which is on the **X Axis**  |     Units displayed in mm or inches     |  <required>   |
| 14. Enter numeric **Width** (based on 0 origin), which is on the **Y Axis**   |     Units displayed in mm or inches     |  <required>   |
| 15. Enter numeric **Height** (based on 0 origin), which is on the **Z Axis**  |     Units displayed in mm or inches     |  <required>   |
| 16. Optional: Enter numeric **Thickness** if solid is hollow or has walls     |     Units displayed in mm or inches     |               |
| 17. Optional: Enter numeric **X Offset**, setting start point on **X Axis**   |     Units displayed in mm or inches     |       0       |
| 18. Optional: Enter numeric **Y Offset**, setting start point on **Y Axis**   |     Units displayed in mm or inches     |       0       |
| 19. Optional: Enter numeric **Z Offset**, setting start point on **Z Axis**   |     Units displayed in mm or inches     |       0       |
| 20. Optional: Enter numeric **Rotate X**, rotating 0-360° on **X Axis**       |     Units displayed in degrees (°)      |       0°      |
| 21. Optional: Enter numeric **Rotate Y**, rotating 0-360° on **Y Axis**       |     Units displayed in degrees (°)      |       0°      |
| 22. Optional: Enter numeric **Rotate Z**, rotating 0-360° on **Z Axis**       |     Units displayed in degrees (°)      |       0°      |

###### 
#### Setting Inputs for New Solid: Cylinder
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 12. Optional: Select from presets to apply cylinder radius or cylinder height |     Screw Size or Screw Type            |               |
| 13. Enter numeric **Radius**, based on center to outer edge                   |     Units displayed in mm or inches     |  <required>   |
| 14. Optional: Enter numeric **Radius1**, based on center to outer edge        |     Units displayed in mm or inches     |               |
| 15. Optional: Enter numeric **Radius2**, based on center to inner edge        |     Units displayed in mm or inches     |               |
| 16. Enter numeric **Height** (based on 0 origin), which is on the **Z Axis**  |     Units displayed in mm or inches     |  <required>   |
| 17. Optional: Enter numeric **X Offset**, setting start point on **X Axis**   |     Units displayed in mm or inches     |       0       |
| 18. Optional: Enter numeric **Y Offset**, setting start point on **Y Axis**   |     Units displayed in mm or inches     |       0       |
| 19. Optional: Enter numeric **Z Offset**, setting start point on **Z Axis**   |     Units displayed in mm or inches     |       0       |
| 20. Optional: Enter numeric **Rotate X**, rotating 0-360° on **X Axis**       |     Units displayed in degrees (°)      |       0°      |
| 21. Optional: Enter numeric **Rotate Y**, rotating 0-360° on **Y Axis**       |     Units displayed in degrees (°)      |       0°      |
| 22. Optional: Enter numeric **Rotate Z**, rotating 0-360° on **Z Axis**       |     Units displayed in degrees (°)      |       0°      |

###### 
#### Object Action Buttons
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. **Create Solid** - save the inputs with select solid type to object        | Required settings inputs for solid      |               |
| 2. **Remove Solid** - remove a selected solid (row) from the object           | Selected row from object details        |               |
| 3. **Import Object** - get all details and items by existing object name      | Object Name                             |               |
| 4. **Clear Object** - clears all inputs, object tables, and object name       |                                         |               |
| 5. **Clear Inputs** - clear out any entered solid inputs                      |                                         |               |
| 6. **View Output** - preview the object in OpenSCAD                           |                                         |               |
| 7. **Modules** - update/add saved solids to object's specific modules         |                                         |               |

###### 
#### Viewing the Object in OpenSCAD
The object, along with any solids, is stored in a **Scad/Solids** folder that is included with the application. The main solid modules are in **object.scad**.

###### 
| Files                                   | Usage In Your SCAD File                                | Optional parameters |
| --------------------------------------- |:------------------------------------------------------:|:-------------------:|
| Scad/Solids/moduleType_name_object.scad | include <object_name_type.scad>;                       |                     |
| Scad/Axes/custom_axis_name.scad         | use <Axes/axes.scad>; Get_Custom_Axis_Name();          | colorVal, alpha     |
| Scad/Solids/object.scad                 |                                                        |                     |
