# CS_COMPILER=mcs
CS_COMPILER=csc
CLR_RUNTIME=mono
EXE=BFC
OS_RUNTIME=osx-x64
PUBLISH_DIR=./release

# make compile bf=./test/hello_world.bf
# compile: bfc
# 	$(CLR_RUNTIME) $< $(bf)
# bfc: bfc.cs Program.cs
# 	$(CS_COMPILER) -out:$@ $^

.PHONY: run clean release
run:
	$(CLR_RUNTIME) $(EXE)
clean: 
	rm -rf bin/ obj/ release/
release:
	dotnet publish -c Release -r $(OS_RUNTIME) -o $(PUBLISH_DIR)
