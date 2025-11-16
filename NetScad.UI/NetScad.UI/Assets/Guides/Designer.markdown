 
# ![Logo](Assets/Images/logo.png) NetSCAD Object Designer Guide
###### 
#### Description
Welcome to the NetSCAD Designer Guide! This tutorial covers the basics of defining and generating custom objects used in OpenSCAD, a solid 3D CAD modeler. This project aims to simplify the process of creating complex 3D models by providing reusable components and utilities. Currently, it includes automating the creation of custom Imperial and Metric axes, as well as quick generation of objects to help speed up the modeling process. This allows for more precise modeling used in 3D printing.
###### 
#### Prerequisites
* **You need to have the following installed:** [OpenSCAD](https://openscad.org/downloads.html)
###### 
#### Types of Solids Available
Cubes and cylinders can be generated from the Designer module within this application. Rounded versions of cubes are also available in this iteration, in order to show how to offset Minkowski rounding to align with 0,0 axes. Future iterations will have more solid types available, but if needed, generated outputs can be modified within SCAD files directly. Solids available are as follows:
###### 
* **Cube** - default aligned on the 0,0,0 axes or can be offset with translate
* **Rounded Cube** - using Minkowski rounding with offsets to align with 0,0,0 axes
* **Cylinder** - all cylinders are aligned on the 0,0,0 axes, with the center point of the top of the cylinder (center circle)
* **Surface** - import from png or dat file. Default aligned on the 0,0,0 axes or can be offset with translate.
###### 
#### Layout of Custom Object Designer
There are 5 main sections: applying a custom axis, creating a new object or retrieving an existing one, setting inputs for new solids, the list of Solids in the object, and Modules that contain the Solids. Once a solid is added to the object, it will appear in the Solids table, along with its parameters. Solids can be viewed within Modules, which define how they interact with each other. To view the object in OpenSCAD, simply click the **View Output** button, which will open the corresponding SCAD file. To export the object as an STL file, click the **STL** button. When making changes, be sure to click the **Update Modules** button to refresh the object.

###### 
![DesignerObject](Assets/Images/designerObjectDark.png)

#### Apply a Custom Axis
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. Open the **Create** menu and select **Create New Object**                  |                                         |               |
| 2. Select an **Axis Type** to filter saved Imperial or Metric axes            |       Metric (mm) or Imperial (in)      |  Metric (mm)  |
| 3. Choose from **Select Axis** to apply one of the saved axes                 |     Dark or Light X x Y x Z mm or in    |  <required>   |
| 4. Optional: Enter numeric **Adjust X**, moving start point on **X Axis**     |     Units displayed in mm or inches     |       0       |
| 5. Optional: Enter numeric **Adjust Y**, moving start point on **Y Axis**     |     Units displayed in mm or inches     |       0       | 
| 6. Optional: Enter numeric **Adjust Z**, moving start point on **Z Axis**     |     Units displayed in mm or inches     |       0       |

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
#### Setting Inputs for New Solid: Surface
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 12. Optional: **Invert**, when importing an image or dat file                 |     Yes | No                            |               |
| 13. Optional: **Smooth**, will apply smoothing to an image and save as new one|     Yes | No                            |               |
| 14. Optional: **Center**, centers image on 0,0,0 axes                         |     Yes | No                            |               |
| 15. Button: **Import File**, browse locally for an image or dat file to import|                                         |               |
| 16. Text Box: displays the path for the file that was imported                |          .png, .data, etc.              |  <required>   |
| 17. **Scale X**, will scale the imported image along the X axis               |     From 0 - 1                          |       1       |
| 18. **Scale Y**, will scale the imported image along the Y axis               |     From 0 - 1                          |       1       |
| 19. **Scale Z**, will scale the imported image along the Z axis               |     From 0 - 1                          |       1       |
| 20. Optional: Enter numeric **X Offset**, setting start point on **X Axis**   |     Units displayed in mm or inches     |       0       |
| 21. Optional: Enter numeric **Y Offset**, setting start point on **Y Axis**   |     Units displayed in mm or inches     |       0       |
| 22. Optional: Enter numeric **Z Offset**, setting start point on **Z Axis**   |     Units displayed in mm or inches     |       0       |
| 23. Optional: Enter numeric **Rotate X**, rotating 0-360° on **X Axis**       |     Units displayed in degrees (°)      |       0°      |
| 24. Optional: Enter numeric **Rotate Y**, rotating 0-360° on **Y Axis**       |     Units displayed in degrees (°)      |       0°      |
| 25. Optional: Enter numeric **Rotate Z**, rotating 0-360° on **Z Axis**       |     Units displayed in degrees (°)      |       0°      |

###### 
#### Object Action Buttons
| Steps                                                                         | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. **Create Solid** - save the inputs with select solid type to object        | Required settings inputs for solid      |               |
| 2. **Import Object** - get all details and items by existing object name      | Object Name                             |               |
| 3. **Clear Object** - clears all inputs, object tables, and object name       |                                         |               |
| 4. **Clear Inputs** - clear out any entered solid inputs                      |                                         |               |
| 5. **View Output** - preview the object in OpenSCAD                           |                                         |               |
| 6. **Remove/Apply Axis** - remove the axis before exporting (or reapply it)   |                                         |               |
| 7. **Modules** - update/add saved solids to object's specific modules         |                                         |               |

###### 
#### Optional Object Actions
| Actions                                                                       | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. **Render ?** - choose whether to pre-render the object before viewing      | Yes (Render) or No (Preview Only)       |               |
| 2. **Export ?** - export a rendered version of the object to output file      | STL                                     |               |

###### 
#### Solids and Modules Tables Actions
| Actions                                                                       | Requirements / Options                  | Default Value |
| ----------------------------------------------------------------------------- |:---------------------------------------:|:-------------:|
| 1. **Trash Bin Icon** - remove the selected row from the object               |       Solids or Modules Table           |               |
| 2. **Clipboard Icon** - opens a modal showing the solids used in the module   | Modules Table                           |               |
| 3. **Sorting** - click on one or more column headers to sort (shift + click)  |       Solids or Modules Table           |               |
| 4. **Color Dropdown** - set the color for solids in the module                |       Modules Table                     |               |

######
#### Rendering/Previewing the Object in OpenSCAD
The object, along with any solids, is stored in a **Scad/Solids** folder that is included with the application. The main solid modules are in **object.scad**. **Rendering the axis is optional, based on your use case.** However, this can SIGNIFICANTLY INCREASE render time as it is primarily used for preview and development. Click the **Remove Axis** within the object.scad file to comment out the axis. If exporting to STL, this will be done automatically. If building a ruler or some form of 1D or 2D measurement, then this is would be an example of why you would include the axis in rendering before final output.

###### 
| Files                                   | Usage In Your SCAD File                                | Optional parameters | Render |
| --------------------------------------- |:------------------------------------------------------:|:-------------------:|:------:|
| Scad/Axes/custom_axis_name.scad         | use <Axes/axes.scad>; Get_Custom_Axis_Name();          | colorVal, alpha     |	No    |
| Scad/Solids/moduleType_name_object.scad | include <object_name_type.scad>;                       |                     |	Yes   |
| Scad/Solids/object.scad                 |                                                        |                     |	No    |	
| Scad/Solids/object.stl                  |                                                        |                     |  Yes   |