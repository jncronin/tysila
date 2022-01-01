extern jit_tm

label:
    cmp dword [label-8 wrt rip], 2    ; short circuit if already compiled
    je label4

    push rdi                    ; else test.  0=NOTDONE, 1=INPROG, 2=DONE
    mov rdi, 1

label1:
    xor eax, eax
    lock cmpxchg [label-8 wrt rip], edi
    cmp eax, 1
    jl label2       ; NOTDONE, now set to INPROG, begin compiling
    jg label3       ; DONE, just run (pop rdi first)
    pause
    jmp label1      ; INPROG, tight spinloop awaiting it to be done
    
label2:
    push rsi
    push rdx
    push rcx
    push r8
    push r9
    push r10
    push r11

    mov rdi, [label-16 wrt rip]
    mov rax, qword jit_tm
    call rax

    pop r11
    pop r10
    pop r9
    pop r8
    pop rcx
    pop rdx
    pop rsi

    mov [label-16 wrt rip], rax
    mov dword [label-8 wrt rip], 2       ; DONE

label3:
    pop rdi

label4:
    jmp [label-16 wrt rip]
