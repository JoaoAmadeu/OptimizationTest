# OptimizationTest
Test to create optimization in three different ways: single thread, multi thread and GPU thread.

Simply take any of the three .cs files on the main folder, rename it to "TestContext.cs" and throw in the Solution folder,
to replace the file already inside. Than create a console application with the updated folder.

-------------------------------------------------------------------------------------------------------------------------

The GPU optimization might not work as intended. I couldn't test properly as my graphics card didn't appear as an option
to run the code.

There is some commented code which force the program to pick a device (CPU, CUDA, OpenCL), so if the current application
don't choose the graphics card, there is the possibility to force through, using those lines of code.
