.PHONY: build restore clean main 

main:| clean restore build
	
clean:
	dotnet clean NetCoreCompiler/NetCoreCompiler/ 
 
restore:
	dotnet restore NetCoreCompiler/NetCoreCompiler/
 
build:
	dotnet build NetCoreCompiler/NetCoreCompiler/
 