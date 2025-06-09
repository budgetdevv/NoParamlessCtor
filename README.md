# NoParamlessCtor

Source generator for blocking use of parameterless constructors! E.x. new SomeStruct() will result in a compile error.

<img width="997" alt="image" src="https://github.com/user-attachments/assets/6f21b59b-e4a5-47d2-b9a1-085a564121b4" />

## Installation

https://www.nuget.org/packages/NoParamlessCtor

## How to use

Declare a `partial` struct with the `[NoParamlessCtor]` attribute

Example:

```cs
[NoParamlessCtor]
public ref partial struct RefCtor(ref PrimaryCtor value)
{
    public ref PrimaryCtor Value = ref value;
}
```

## Usecases

- Structs that will have an invalid state if `new()`-ed. Note that it is still possible for the end-user to declare `default(SomeStruct)`.
