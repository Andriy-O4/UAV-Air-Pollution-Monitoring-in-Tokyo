# UAV-Air-Pollution-Monitoring-in-Tokyo
A repository containing the necessary Unity project files to run my individual project in its final form. 

To run the project you first have to download the files which can be done by clicking on the green 'Code' button to the left of the 'About' section and selecting 'Download ZIP'. 

From there, you should extract the ZIP file wherever you wish and then create a new empty Unity project if possible using the 2022.3.60f1 version of the Unity editor. Finally, simply find the location of your new empty Unity project and drag the downloaded and unzipped project files into there. 

Once downloaded, you should be able to see the mesh of the Chuo ward in Tokyo. In the Unity Hierarchy window you will see a variety of different game objects which can be altered depending on the part of the project you wish to test. The second pollutant field for the 'tractor' UAV mobility pattern is activated by default (alongside the required buildings for that flow field to which the doublet flow fields are attached to mimic the realistic pollutant flow around them). This default activation should act as a template for activating and testing other parts of the code. 

If you wish to activate any of the UAVs with the PDNAs then you need to enable the regular pollutant field (i.e. 'Pollutant1), the relevant flow manager (i.e. 'P1FlowManagerPDNA') and then the relevant buldings for that case which would be all of the buildings up until the next pollutant field - 'Pollutant2' (i.e. 'Building1CentreApprox', 'Building2CentreApprox' and 'Building3CentreApprox'). Finally, you will have to activate the respective UAV at the bottom of the Hierarchy window labelled - 'UAVPDNA1'. 

You can of course run all of the PDNA algorithms at once at all of the pollutant field locations or all of the tractor algorithms but it is not recommended to do both of the mobility patterns at once for performance reasons.

As for the .csv files. The 'ConcMap' files record EVERY pollutant particle detection instance. The 'MaxMap' files only record the local maxima and the 'Results' files record the algorithm performance - for which the tractor algorithm files should have the headings from left to right as: 'Final Distance to Source / m', 'Time to Best Maximum / s', 'Detected maxima' and 'False positives'. 

If there are any issues with running the project please feel free to contact me! 
