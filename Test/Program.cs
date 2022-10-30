using Gander;

GanderProcessor processor = new GanderProcessor();

const string code = @"
fn prompt
    call stdout
    call stdin
fne

fn log
    call stdout
    call endl
fne

lbl start

ld.str ""Do you wish to continue? [Y/n] ""
call prompt

ld.str ""y""
br.ne n

ld.str ""Nice, you continued!""
br end

lbl n
ld.str ""Ok, you did not decide to continue.""

lbl end
call log
ld.str ""Thanks for using this program.""
call log
br start
";

processor.PreProcess(code);
processor.Process(code);