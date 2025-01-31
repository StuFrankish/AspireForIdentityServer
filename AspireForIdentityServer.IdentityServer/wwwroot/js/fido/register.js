const signInButton = document.getElementById("register-fido");

if (signInButton) {
    signInButton.addEventListener("click", (event) => {
        event.preventDefault(); // Prevent default link behavior
        console.log("Register Passkey sign-in clicked");
    });
} else {
    console.warn("Element with ID 'register-fido' not found.");
}