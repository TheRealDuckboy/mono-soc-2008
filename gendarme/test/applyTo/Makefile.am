applyTodir=
applyTo_SCRIPTS = 
applyTo_DATA = 
EXTRA_DIST = 
CLEANFILES = Test.ApplyTo.dll FakeAssembly.dll
DISTCLEANFILES = Makefile.in TestResult.xml

applyTo_sources =  \
	ApplyTo.cs \
	ApplyToAll.cs \
	ApplyToDefault.cs \
	ApplyToNonVisible.cs \
	ApplyToVisible.cs \
	ApplyToMixed.cs \
	FakeRules.cs \
	TestableRunner.cs

applyTo_build_sources = $(addprefix $(srcdir)/, $(applyTo_sources))

fakeAssembly_sources = FakeAssembly.cs
fakeAssembly_build_sources = $(addprefix $(srcdir)/, $(fakeAssembly_sources))

all:Test.ApplyTo.dll

Test.ApplyTo.dll: $(applyTo_build_sources) $(applyTo_SCRIPTS) FakeAssembly.dll 
	$(GMCS) -target:library -pkg:mono-cecil -pkg:mono-nunit -pkg:gendarme-framework \
		-r:/home/nestor/opt/mono/lib/gendarme/gendarme.exe \
		-out:$@ $(applyTo_build_sources)


FakeAssembly.dll: $(fakeAssembly_build_sources) $(applyTo_SCRIPTS)
	$(GMCS) -target:library -out:$@ $(fakeAssembly_build_sources)

run-test: Test.ApplyTo.dll
	MONO_PATH=/home/nestor/opt/mono/lib/gendarme:$(MONO_PATH) nunit-console2 Test.ApplyTo.dll
