# RecieptJsonParser — Receipt OCR JSON Parser

## 📝 Problem

A scanned receipt image is processed by an OCR SaaS API, returning a JSON response containing:
- `description`: The recognized text string
- `boundingPoly.vertices`: The coordinates of the text's bounding box on the image

The goal is to reconstruct the text lines **as they visually appear** on the receipt image by parsing and organizing this JSON accurately.

---

## 🔧 Implementation

- **Input:** A JSON array of OCR blocks with description and bounding box data
- **Output:** A plain text file where each line reflects the physical order on the receipt
- **Sorting Strategy:**
  - Sort text blocks by vertical (Y) position to group them into lines
  - Within each group, sort by horizontal (X) position to reconstruct complete lines

---

## ⚙️ Features

- `OcrService.ReadFileData()`: Main method to convert and organize the input JSON
- Robust coordinate clustering logic with tolerance to handle OCR deviations
- Structured line output written directly to `.txt`

---

## ❓ Note on Filtering Text Blocks

Although no issue was directly encountered during parsing, a potential problem could arise from irregular bounding box sizes in the OCR JSON — particularly if product boxes were significantly smaller or larger than the average height.

### 🔍 Question:
**What should be done if product bounding boxes are significantly smaller or larger than the average height?**

### ✅ Answer:

Inspired by Tesseract's internal logic (see references below), the following thresholds were used:

- If a bounding box is **larger than 2× the average height**, it is considered *noise* and filtered out.
- If a bounding box is **smaller than 0.5× the average height**, it is also considered non-textual or irrelevant.

These thresholds are derived from:
- Tesseract whitepaper: *An overview of the Tesseract OCR Engine*, Section 5–8 — Baseline Fitting and Word Detection
- Tesseract source code references:
  - [kMaxTableCellXheight = 2.0](https://github.com/tesseract-ocr/tesseract/blob/ade0dfaa8cc1b12341286aa91e11f8ab77a035ad/src/textord/tablefind.cpp#L81)
  - [kAllowBlobHeight = 0.3](https://github.com/tesseract-ocr/tesseract/blob/ade0dfaa8cc1b12341286aa91e11f8ab77a035ad/src/textord/tablefind.cpp#L56)

Filtered items are stored under `BlobFilteredOcrTextBlocks`.

Only the remaining OCR blocks — suitable in size — are used to construct a baseline and then grouped into lines via `lineOcrTextBlockDictionary`.

Once all valid blocks are processed, remaining blobs (if any) are appended to the end of the output for completeness.

---
## 🧭 Understanding the Vertex Index Order

Each OCR block in the JSON includes a `boundingPoly.vertices` array, which contains the coordinates of the rectangle surrounding a piece of text.

### 🔢 Vertex Index Convention

The typical order of the 4 vertices is:

![Vertex Index Diagram](./Docs/vertex_index_diagram.png)

| Index | Description        |
|-------|--------------------|
| 0     | Top-left corner    |
| 1     | Top-right corner   |
| 2     | Bottom-right corner|
| 3     | Bottom-left corner |

---

## ▶️ Example Usage

```csharp
OcrService.WriteToText();
```
---

## 📚 References

The following references were studied and partially implemented to ensure robust text line extraction, skew correction, and baseline fitting from OCR-parsed JSON:

1. **Ray Smith**, *An Overview of the Tesseract OCR Engine*, Google Inc., ICDAR 2007.  

2. **Ray Smith**, *A Simple and Efficient Skew Detection Algorithm via Text Row Accumulation*, HP Laboratories Bristol, 1994.  

3. **Akhil S**, *An Overview of Tesseract OCR Engine*, NIT Calicut, 2016.  

4. **Andrew D. Bagdanov**, *Algorithms for Document Image Skew Estimation*, University of Nevada, Las Vegas, 1996.  

5. **Joost van Beusekom, Faisal Shafait, Thomas Breuel**, *Combined Orientation and Skew Detection Using Geometric Text-Line Modeling*.  

6. **Ray Smith**, Extracted Text Summary of Skew Detection Algorithm.  

---

These references form the academic and algorithmic foundation of blob filtering, skew correction, and robust baseline alignment logic implemented in the receipt parser.
