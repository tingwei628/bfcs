CS_COMPILER=mcs
CLR_RUNTIME=mono
EXE=bfAsm.exe

# make compile bf=./test/hello_world.bf
compile: bfc
	$(CLR_RUNTIME) $< $(bf)
bfc: bfc.cs main.cs
	$(CS_COMPILER) -out:$@ $^

.PHONY: run clean
run:
	$(CLR_RUNTIME) $(EXE)
clean: 
	rm -f bfc $(EXE)
