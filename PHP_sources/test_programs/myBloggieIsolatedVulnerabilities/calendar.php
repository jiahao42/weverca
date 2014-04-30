<?php

/**
 * Mimic incorrect handling of constructions found in file calendar.php of 
 * myBloggie web application
 */

if (isset($_GET['month_no'])) { 
    $month = intval($_GET['month_no']);
}
else{ 
    $month=gmdate('n', time() ); 
}

if (isset($_GET['year'])) { 
    $year = intval($_GET['year']);
}
else { 
    $year = gmdate('Y', time() ); 
}

if ($month < 1 || $month > 12) die();

$montht = date('F', mktime(0, 0, 0, $month, 1, $year));

// Pixy reports false positive here
// Pixy is not able to detect that it is accessed only defined element of the
// $lang array.
echo $lang["$montht"];
// Error: access to the uninitialized element of the array
echo $lang["$montht"."a"];
?>
