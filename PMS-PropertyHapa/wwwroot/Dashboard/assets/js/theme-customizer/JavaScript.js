<script>
    document.addEventListener('DOMContentLoaded', function () {
    // Retrieve primary and secondary colors from localStorage
    const primaryColor = localStorage.getItem('primaryColor');
    const secondaryColor = localStorage.getItem('secondaryColor');

    // Apply the primary color as the background color of the body
    if (primaryColor) {
        document.body.style.backgroundColor = primaryColor;
    }

    // Apply the secondary color as the font color for the body
    if (secondaryColor) {
        document.body.style.color = secondaryColor;
    }
});
</script>
