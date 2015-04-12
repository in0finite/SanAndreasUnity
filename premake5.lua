newoption {
    trigger = "protogen",
    description = "Generate ProtoBuf serialization classes"
}

local protoGen = _OPTIONS["protogen"]

local function joinPath(...)
    local args = arg or {...}

    if #args == 0 then return nil end
    if #args == 1 then return args[1] end

    return path.join(args[1], joinPath(select(2, unpack(args))))
end

--
-- ProtoBuf generation
--

local function protoGen(projPath)
    local exePath = path.translate(path.getabsolute(joinPath("Tools", "CodeGenerator.exe")))
    local assPath = path.getabsolute(joinPath(projPath, "Assets"))
    local outPath = joinPath("Scripts", "Generated", "ProtocolBuffers.cs")

    local files = { joinPath(assPath, "ProtoBuf"), joinPath(assPath, "Scripts") }

    for i, f in ipairs(files) do
        files[i] = string.format('"%s"', f)
    end

    local mono = ""

    if os.get() ~= "windows" then mono = "mono " end

    local format = "cd \"%s\" && %s\"%s\" %s --output \"%s\" --preserve-names"
    local call = string.format(format, assPath, mono, exePath, table.concat(files, " "), outPath)

    print(call)

    local pwd = _WORKING_DIR

    local exitCode = os.execute(call)
    os.execute(string.format("cd \"%s\"", pwd))

    return exitCode == 0
end

if protoGen then
	local protoGenSuccess = protoGen(".")
    if protoGenSuccess then os.exit(0) else os.exit(1) end
end

os.exit(0)
