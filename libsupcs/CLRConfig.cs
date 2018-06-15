/* Copyright (C) 2018 by John Cronin
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

namespace libsupcs
{
    public class CLRConfig
    {
        /* Implement as two separate functions to allow more advanced systems to
         * override GetConfigBoolValue with a non-weak linkage function to handle
         * config values it knows about, and then call the public GetConfigBool()
         * below for ones it doesn't */
        public static bool GetBoolValue(string name)
        {
            if (name == "System.Globalization.Invariant")
                return true;
            return false;
        }

        [WeakLinkage]
        [AlwaysCompile]
        [MethodAlias("_ZW6System9CLRConfig_18GetConfigBoolValue_Rb_P1u1S")]
        static bool GetConfigBoolValue(string name)
        {
            return GetBoolValue(name);
        }

        unsafe struct StringPtrOnStack
        {
            internal void** ptr;
        }

        [AlwaysCompile]
        [MethodAlias("_ZW18System#2EResources29ManifestBasedResourceGroveler_36GetNeutralResourcesLanguageAttribute_Rb_P3U19System#2EReflection15RuntimeAssemblyU35System#2ERuntime#2ECompilerServices19StringHandleOnStackRs")]
        static unsafe bool ManifestBasedResourceGroveler_GetNeutralResourcesLanguageAttribute(TysosAssembly a, StringPtrOnStack cultureName, out short fallbackLocation)
        {
            *cultureName.ptr = CastOperations.ReinterpretAsPointer("");
            fallbackLocation = 0;
            return false;
        }

        [AlwaysCompile]
        [MethodAlias("_ZW6System9Exception_29GetMessageFromNativeResources_Rv_P2V32Exception#2BExceptionMessageKindU35System#2ERuntime#2ECompilerServices19StringHandleOnStack")]
        static unsafe void GetMessageFromNativeResources(int kind, StringPtrOnStack msg)
        {
            switch(kind)
            {
                case 1:
                    *msg.ptr = CastOperations.ReinterpretAsPointer("ThreadAbort");
                    break;
                case 2:
                    *msg.ptr = CastOperations.ReinterpretAsPointer("ThreadInterrupted");
                    break;
                case 3:
                    *msg.ptr = CastOperations.ReinterpretAsPointer("OutOfMemory");
                    break;
                default:
                    *msg.ptr = CastOperations.ReinterpretAsPointer("Unknown Exception (GetMessageFromNativeResources)");
                    break;
            }
        }
    }
}
