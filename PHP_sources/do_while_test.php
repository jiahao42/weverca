<?
do{
	$a=$a+$a;
	echo "cycle";
	do
	{
		$a=$a*$a;
		echo "inner cycle";
	}while("true"=="true");
	$a=8;
	echo "cycle";
}while("true"=="true");
echo "end";


?>