const signInButton = document.getElementById("signin-fido");

if (signInButton) {
    signInButton.addEventListener("click", (event) => {
        event.preventDefault(); // Prevent default link behavior
        console.log("Passkey sign-in clicked");
    });
} else {
    console.warn("Element with ID 'signin-fido' not found.");
}