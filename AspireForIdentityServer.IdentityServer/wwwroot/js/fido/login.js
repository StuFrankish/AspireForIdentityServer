const signInButton = document.getElementById("signin-fido");
const loginForm = document.getElementById("login-form");

if (signInButton) {
    signInButton.addEventListener("click", async (event) => {

        const { returnUrl } = Object.fromEntries(new FormData(loginForm));

        console.log(returnUrl);

        let response = await fetch("/fido2/createassertionoptions", {
            headers: { "Content-Type": "application/json" },
            method: "POST"
        });

        if (!response.ok) {
            throw new Error("Failed to create the assertion options.");
        }

        const publicKey = await response.json();

        publicKey.challenge = coerceToArrayBuffer(publicKey.challenge);

        for (const allowCredential of publicKey.allowCredentials) {
            allowCredential.id = coerceToArrayBuffer(allowCredential.id);
        }

        const credential = await navigator.credentials.get({ publicKey });

        response = await fetch("/fido2/createassertion", {
            body: JSON.stringify({
                id: credential.id,
                rawId: coerceToBase64Url(credential.rawId),
                type: credential.type,
                extensions: credential.getClientExtensionResults(),
                response: {
                    authenticatorData: coerceToBase64Url(credential.response.authenticatorData),
                    clientDataJSON: coerceToBase64Url(credential.response.clientDataJSON),
                    signature: coerceToBase64Url(credential.response.signature)
                }
            }),
            headers: { "Content-Type": "application/json" },
            method: "POST"
        });

        if (!response.ok) {
            throw new Error("Failed to create the assertion.");
        }

        window.location.assign(returnUrl ?? '/');

    });
} else {
    console.warn("Element with ID 'signin-fido' not found.");
}