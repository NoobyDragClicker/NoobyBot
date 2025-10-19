# Makefile for building a self-contained C# engine for OpenBench
# Usage: make EXE=Engine-ABCDEFGH

# Default output name
EXE ?= NoobyBot

# Your C# project file
PROJECT = src/ChessEngine.csproj

# Build configuration and target framework
CONFIG = Release
FRAMEWORK = net8.0

# Runtime identifier (change if you're building for Linux)
# Common values: win-x64, linux-x64, osx-x64
RID = win-x64

# Determine file extension (for Windows/Linux compatibility)
ifeq ($(findstring win,$(RID)),win)
    EXT = .exe
else
    EXT =
endif

# Default build rule
all:
	dotnet publish $(PROJECT) \
		-c $(CONFIG) \
		-f $(FRAMEWORK) \
		-r $(RID) \
		--self-contained true \
		/p:PublishSingleFile=true \
		/p:IncludeNativeLibrariesForSelfExtract=true \
		-o build
	cp build/$(basename $(notdir $(PROJECT))).$(RID)$(EXT) $(EXE)$(EXT) 2>/dev/null || \
	cp build/$(basename $(notdir $(PROJECT)))$(EXT) $(EXE)$(EXT)

clean:
	rm -rf build
	rm -f $(EXE)$(EXT)
