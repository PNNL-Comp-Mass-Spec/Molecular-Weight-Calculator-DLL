xcopy \\proto-2\CI_Publish\MolecularWeightCalculatorDll\MolecularWeightCalcCOMInstaller.exe . /Y /D

"C:\Program Files\7-Zip\7z.exe" a MolecularWeightCalculatorDll.zip ..\Readme.md ..\RevisionHistory.txt \\proto-2\CI_Publish\MolecularWeightCalculatorDll\MolecularWeightCalculator.dll \\proto-2\CI_Publish\MolecularWeightCalculatorDll\MolecularWeightCalculator32.tlb \\proto-2\CI_Publish\MolecularWeightCalculatorDll\MolecularWeightCalculator64.tlb \\proto-2\CI_Publish\MolecularWeightCalculatorDll\MolecularWeightCalculator.xml

pause