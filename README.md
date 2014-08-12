DynareRemoveLocalVariables
==========================

An alternative preprocessor to remove model local variables from a .mod file.

Command line:

DynareRemoveLocalVariables inputfile.mod [outputfile.mod] [1]

The optional final argument (if present) tells the parser to convert lines starting with @ to comments.
This is useful if running on the output from Dynare's preprocessor.

