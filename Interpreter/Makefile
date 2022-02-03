# CS_COMPILER=mcs
CS_COMPILER=csc
CLR_RUNTIME=mono
EXE=bfi

bfi: bfi.cs
	$(CS_COMPILER) -out:$@ $<

.PHONY: run clean
# make run bf=./test.bf
run:
	$(CLR_RUNTIME) $(EXE) $(bf)
clean: 
	rm -f $(EXE)
