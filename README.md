# AI-Driven SDLC Guidelines Repository

## 🎯 Objective

This repository demonstrates how to adopt and leverage AI tools to optimize every step in the Software Development Life Cycle (SDLC) process. It provides a detailed, hands-on approach with tool-specific examples, prompts, and best practices for building software with AI assistance.

## 🚀 What You'll Learn

- How to integrate AI tools into each phase of the SDLC
- Practical prompts and techniques for AI-assisted development
- End-to-end implementation of a complete Todo service using .NET 8 WebAPI
- Best practices for AI-driven requirements gathering, planning, coding, testing, and documentation

## 📚 Repository Structure

### 📁 `docs/`
Core documentation and guidelines for AI-driven SDLC process:

- **`guidelines.md`** - Main comprehensive guide covering all SDLC phases
- **`initial_requirements.md`** - Starting requirements for the Todo app example
- **`final_requirements.md`** - Refined requirements after AI-assisted clarification
- **`plan.md`** - AI-generated user stories, backlog, and sprint planning
- **`tech_design_res.md`** - Technical design and architecture decisions

### 📁 `prompts/`
Reusable AI prompts for different SDLC phases:

- **`tech_design_prompt.md`** - Prompt for generating technical design documents
- **`document_prompt.md`** - Prompt for comprehensive code documentation
- Additional prompts for requirements analysis, code review, and testing

### 📁 `src/`
Example Todo application implementation demonstrating AI-assisted development:

```
src/
├── TodoApp.Api/          # Presentation layer (Controllers, Views, Middleware)
├── TodoApp.Application/  # Business logic (Services, Validators)
└── TodoApp.Infrastructure/ # Data access (Repositories, DbContext, Entities)
```

### 📁 `tests/`
Comprehensive test suite generated with AI assistance:

```
tests/
└── TodoApp.Tests/
    ├── Api/           # Controller and API endpoint tests
    ├── Application/   # Service and business logic tests
    ├── Infrastructure/# Repository and data access tests
    └── Ui/           # End-to-end UI tests with Playwright
```

## 🔄 SDLC Phases Covered

### Phase 0: Request for Proposal
- Initial requirements gathering and stakeholder communication

### Phase 1: Requirements & Planning
- **AI-assisted requirement clarification** - Generate clarifying questions
- **Story creation** - Convert requirements to INVEST user stories with Gherkin criteria
- **Sprint planning** - Organize backlog and create development sprints

### Phase 2: Design & Architecture
- **Technical design** - AI-generated architecture decisions and tech stack selection
- **Visual documentation** - Mermaid diagrams for system flows and components

### Phase 3: Development & Coding
- **Iterative development** - Task-by-task implementation with AI assistance
- **Code generation** - Full-stack implementation following architectural patterns

### Phase 4: Testing & Quality Assurance
- **Unit testing** - Comprehensive test suite generation
- **UI testing** - End-to-end tests with Playwright
- **Coverage analysis** - Test coverage reporting and gap identification

### Phase 5: Code Review
- **AI-assisted review** - Automated analysis of code changes and quality assessment
- **Best practices** - Style, security, and performance recommendations

### Phase 6: Documentation
- **Code documentation** - Inline comments, XML documentation, and API specs
- **Project documentation** - README files and architectural guides

## 🛠️ Technology Stack

The example implementation uses:

- **.NET 8 WebAPI** - Backend framework
- **Entity Framework Core** - Data access with SQLite
- **Razor Views + Vanilla JavaScript** - Frontend UI
- **xUnit + FluentAssertions + Moq** - Unit testing
- **Playwright** - End-to-end UI testing

> **Note**: The technology stack is flexible - you can adapt the prompts and techniques to any programming language or framework.

## 🚀 Getting Started

- Go to the [Guidelines document](docs/guidelines.md)
- Follow the guidelines steps to regenerate the full experience and artifacts of this repo using your own AI Agent and tech stack. 
- This enforces the steps of the SDLC process
- This demonstrates how AI agents enhances the analysis, learning and results in each step. 
- This creates an isolated environment to experiment with different AI tools and practices, mitigating the fear and resistance of messing your current work environment. 


## 🤖 Compatible AI Tools

This methodology works with various AI assistants, it is basically AI tool agnostic. 

## 🎯 Key Benefits

- **Faster Development** - Reduce implementation time with AI assistance
- **Better Quality** - Comprehensive testing and documentation generation
- **Consistent Practices** - Standardized approaches across team members
- **Knowledge Transfer** - AI helps capture and share architectural decisions
- **Reduced Errors** - Automated testing and review processes

## 🤝 Contributing

This repository serves as a learning resource and reference implementation. Feel free to:

- Adapt the methodology to your technology stack
- Contribute additional prompts and examples
- Share improvements and lessons learned
- Report issues or suggest enhancements

---

**Remember**: This is not an exhaustive guide but a practical starting point. The key is to unleash your imagination and creativity, partnering with AI at every step to expand your knowledge, sharpen your skills, and amplify your impact.