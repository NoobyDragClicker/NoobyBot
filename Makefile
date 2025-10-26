# Makefile for building a self-contained C# engine for OpenBench
# Usage:
#   make                → auto-detects OS and builds
#   make RID=linux-x64  → override manually
#   make clean

# Default output name
EXE ?= NoobyBot

# Your C# project file
PROJECT = src/ChessEngine.csproj

# Build configuration and target framework
CONFIG = Release
FRAMEWORK = net8.0

# Detect platform automatically (unless overridden)
UNAME_S := $(shell uname -s)
ifeq ($(UNAME_S),Linux)
	RID ?= linux-x64
	EXT =
else ifeq ($(UNAME_S),Darwin)
	RID ?= osx-x64
	EXT =
else
	RID ?= win-x64
	EXT = .exe
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
	cp -f build/$(basename $(notdir $(PROJECT))).$(RID)$(EXT) $(EXE)$(EXT) 2>/dev/null || \
	cp -f build/$(basename $(notdir $(PROJECT)))$(EXT) $(EXE)$(EXT)

clean:
	rm -rf build
	rm -f $(EXE)$(EXT)