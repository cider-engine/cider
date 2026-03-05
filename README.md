# Cider Engine

<del>Cider引擎正在开发中，可能随时进行破坏性更改，请勿将其用于开发！</del>

<del>孩子们，MonoGame的bug修不完，迟迟不更新，哥们儿跑路玩Godot去了，有缘再见，adios</del>

孩子们我肘赢复活赛了

> 我不拥有Cider.Test的测试素材版权，它们只是我在测试导入时遗留下来的，请勿商用这些素材！

## 目前已知bug

### WebAssembly

- [ ] [调用关于基于64位整数的枚举的WASM原生函数时出现函数签名不匹配](https://github.com/dotnet/runtime/issues/112262)
- [ ] [(已修复待同步)`@(WasmFilesToIncludeInFileSystem)`无法将文件正常包含进VFS](https://github.com/dotnet/runtime/pull/120970)
- [ ] 一旦调用`MIX_SetTrackStoppedCallback`就会崩溃
    > 猜测为undefined symbol问题
- [ ] nuget包随附Emscripten SDK版本过低
    > SDL_ttf在使用freetype库加载字体时会使用setjmp（___wasm_setjmp），此函数位于emscripten compiler-rt (Runtime Library)中，3.1.57版本及以后支持，目前最新nuget包随附emscripten sdk版本为3.1.56