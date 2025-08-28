UpperBorder: .word 1000
i: .word 0

start:
	LDA #1
	STA i
WHILE_0:
	LDA i
	PHA
	LDA UpperBorder
	PLA
	CMP
	BCC LE_LABEL
	BEQ LE_LABEL
	CMP #0
	BEQ ENDWHILE_0
	LDA i
; Print value in A (user must implement print routine)
	JSR PRINT
	LDA i
	PHA
	LDA #1
	PLA
	CLC
	ADC
	STA i
	JMP WHILE_0
ENDWHILE_0:
	BRK

subroutine:
	LDA #"Jumped to subroutine."
; Print value in A (user must implement print routine)
	JSR PRINT
	RTS
