using System;
using System.Threading.Tasks;
using Redakt.BackOffice.Onboarding;
using RedaktHotel.BackOfficeExtensions.Components;

namespace RedaktHotel.BackOfficeExtensions
{
    public class CustomOnboardingStep: IBackOfficeOnboardingStep
    {
        public Task<OnboardingStepType> RequiresOnboardingAsync()
        {
            return Task.FromResult(OnboardingStepType.Optional);
        }

        public Type ComponentType => typeof(WelcomeOnboard);

        public string StepName => "The Redakt Hotel";

        public int Order => 3;
    }
}
