function validates(password)
{
    var format = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[#@$%&])(.{6,20}$)/;
    if (!(format.test(password))) {
        return false;
    }
    else {
        return true;
    }
}