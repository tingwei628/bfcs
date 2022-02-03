# CS_COMPILER=mcs
CS_COMPILER=csc
CLR_RUNTIME=mono
EXE=BFC
OS=osx-x64

# make compile bf=./test/hello_world.bf
# compile: bfc
# 	$(CLR_RUNTIME) $< $(bf)
# bfc: bfc.cs Program.cs
# 	$(CS_COMPILER) -out:$@ $^

.PHONY: run clean release
run:
	$(CLR_RUNTIME) $(EXE)
clean: 
	rm -rf bfc $(EXE) bin/ obj/
release:
	dotnet publish -c Release -r $(OS)
