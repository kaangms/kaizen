# 🔐 Brute-Force Collision Probability

When generating a set of valid codes (e.g., 15 million) from a large space of possible codes (e.g., `23^8 ≈ 78.3 billion`), an attacker attempting brute-force guessing may eventually land on a valid code.

### 🎯 Problem

- Total code space: `N = 78,310,985,281` (all possible 8-char codes using 23-char alphabet)
- Valid generated codes: `K = 15,000,000`
- Brute-force attempts: `T = 1,000,000`

### 🧮 Collision Probability Formula

\[
P(\text{at least one success}) = 1 - \left(1 - \frac{K}{N} \right)^T
\]

This is a classic **Bernoulli trial** model in finite probability space, used to estimate success in repeated random draws.

---

### 🔢 Applying Values

\[
P = 1 - \left(1 - \frac{15,000,000}{78,310,985,281} \right)^{1,000,000}
\]

\[
≈ 1 - e^{- \frac{15,000,000 × 1,000,000}{78,310,985,281}} ≈ 1 - e^{-191.5} ≈ 1
\]

---

### ✅ Result

> If an attacker makes 1 million random guesses, the chance of guessing **at least one valid code** is **nearly 100%**.

Even a relatively small set of valid codes becomes highly guessable when brute-force is applied at scale.

---

### 🔒 Recommendations to Mitigate Brute-Force Risk

To reduce brute-force vulnerability:
- Expand the Character Set (Base > 23)
- Keep the validation logic on the server side.
- Increase Code Length
- As the number of generated codes decreases, the probability of collisions also decreases. 

---

## 📚 References

- Preshing, Jeff. [**Hash Collision Probabilities**](https://preshing.com/20110504/hash-collision-probabilities/)  
- Uniform random sampling over finite space: [Wikipedia – Bernoulli trial](https://en.wikipedia.org/wiki/Bernoulli_trial)


