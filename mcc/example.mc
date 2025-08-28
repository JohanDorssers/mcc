const integer UpperBorder = 1000;

var i: integer;

start {
	i = 1;
	while (i <= UpperBorder) {
		print i;
		i = i + 1;
	}
}

sub subroutine {
	print "Jumped to subroutine.";
}
