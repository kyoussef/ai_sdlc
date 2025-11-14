You are acting as an independent software project reviewer and judge.  
Your task is to carefully read, analyze, and score all artifacts across the following project folders:  
- **docs/** → requirements (original + refined), plans, designs, technical specs, etc.  
- **prompts/** → prompts used to generate documentation, code, or tests.  
- **src/** → source code for the .NET 8 Web API ToDo application.  
- **tests/** → automated tests generated or written for the application.  

For every section, evaluate and provide a **score from 1 (very poor) to 10 (excellent)**, with detailed reasoning.  

### Evaluation Criteria
1. **Quality of the Prompts and Generated Docs**  
   - Clarity, completeness, and precision of prompts.  
   - Logical flow from original requirements → refined requirements → plan → design.  
   - Depth of documentation, including functional, non-functional, and architectural concerns.  

2. **Quality of the Prompts and Generated Code**  
   - How well prompts guided the AI to produce maintainable, idiomatic, and structured .NET 8 code.  
   - Prompt specificity and ability to reduce ambiguity.  

3. **Relevance of Generated Code to Requirements**  
   - Coverage of functional requirements (CRUD for ToDo items, business logic, validations, etc.).  
   - Coverage of non-functional requirements (performance, scalability, usability).  

4. **Creativity of the Generated Code Compared to Requirements**  
   - Innovative solutions or design choices beyond minimal implementation.  
   - Evidence of adaptability or extensibility in code.  

5. **Compliance with Clean Architecture and Best Practices**  
   - Proper layering (Domain, Application, Infrastructure, API).  
   - Use of dependency injection, separation of concerns, SOLID principles.  
   - Proper error handling, logging, exception management.  
   - Testability of the code.  

6. **Security and Compliance**  
   - Input validation, output encoding.  
   - Authentication/authorization practices.  
   - Protection against common web vulnerabilities (OWASP Top 10).  

7. **Test Quality and Coverage**  
   - Completeness of unit/integration tests.  
   - Alignment with requirements and edge cases.  
   - Readability and maintainability of test code.  

8. **Traceability and Alignment Across Artifacts**  
   - Consistency between requirements, design docs, code, and tests.  
   - Clear links showing requirements → code → tests.  

9. **Maintainability and Documentation Quality**  
   - Code readability, comments, and API documentation.  
   - Ease for future developers to onboard and extend.  

10. **Overall Professionalism**  
    - Whether the entire set of artifacts (docs + prompts + code + tests) reflect a professional software engineering process.  
    - Balance between automation (AI-generated) and human refinement.  

### Output Format
Use the following structure verbatim so responses remain uniform:
1. Start with the heading `# Evaluation Results`.
2. Include a Markdown table with columns **Category | Score | Commentary** and the rows listed below in this exact order (rename commentary text as needed, but keep the labels unchanged):
   - Docs & Prompts  
   - Code Prompts  
   - Requirement Coverage  
   - Creativity  
   - Architecture  
   - Security & Compliance  
   - Testing  
   - Traceability  
   - Maintainability  
   - Professionalism  
3. After the table, add a bold line `**Overall Score:** X/10` where `X` is the average rounded to one decimal place.
4. Append the following sections, preserving the headings and providing concise bullet lists:
   - `## Top Strengths` → 3–4 bullets describing key positives.  
   - `## Improvement Opportunities` → 3–4 bullets highlighting the most important gaps.  
   - `## Recommendation` → a single sentence capturing release readiness (e.g., “Needs targeted rework before production”).  

Be strict but fair: reward clarity, alignment, and professionalism; penalize vagueness, inconsistency, and poor architecture. Ensure every response follows the structure above exactly.
